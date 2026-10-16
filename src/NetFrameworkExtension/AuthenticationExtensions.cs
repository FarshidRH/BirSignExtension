using MapIdeaHub.BirSign.NetFrameworkExtension.Helpers;
using MapIdeaHub.BirSign.NetFrameworkExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using MapIdeaHub.BirSign.SharedKernel.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace MapIdeaHub.BirSign.NetFrameworkExtension
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Enables BirSign authentication using OpenID Connect in the OWIN application pipeline.
        /// </summary>
        public static IAppBuilder UseBirSignAuthentication(
            this IAppBuilder app,
            Func<ClaimsIdentity, Task> manageUser = null,
            Action<OpenIdConnectAuthenticationOptions> optionsConfigurator = null,
            string webhookUrl = "/api/birsign/webhook",
            Func<MapIdeaHub.BirSign.SharedKernel.Dtos.WebhookEvent, Task> webhookHandler = null)
        {
            var options = GetDefaultOpenIdConnectAuthenticationOptions(manageUser);
            optionsConfigurator?.Invoke(options);

            BirSignSettings.SetAuthority(options.Authority);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = options.TokenValidationParameters.NameClaimType;
            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ApplicationCookie);

            if (webhookHandler != null && !string.IsNullOrEmpty(webhookUrl))
            {
                app.Use<WebhookOwinMiddleware>(webhookUrl, webhookHandler);
            }

            app.UseOpenIdConnectAuthentication(options);

            // Back-channel logout session invalidation OWIN middleware
            app.Use(async (context, next) =>
            {
                if (context.Authentication?.User?.Identity?.IsAuthenticated == true)
                {
                    var sub = context.Authentication.User.FindFirst("sub")?.Value 
                              ?? context.Authentication.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(sub) && MemoryCache.Default.Contains($"RevokedUser_{sub}"))
                    {
                        context.Authentication.SignOut(
                            DefaultAuthenticationTypes.ApplicationCookie,
                            DefaultAuthenticationTypes.ExternalCookie,
                            BirSignConstants.AuthenticationType);

                        context.Response.Redirect(context.Request.Uri.PathAndQuery);
                        return;
                    }
                }

                await next();
            });

            return app;
        }

        private static OpenIdConnectAuthenticationOptions
            GetDefaultOpenIdConnectAuthenticationOptions(Func<ClaimsIdentity, Task> manageUser)
        {
            return new OpenIdConnectAuthenticationOptions
            {
                Authority = ConfigurationManager.AppSettings["BirSign:Authority"],
                ClientId = ConfigurationManager.AppSettings["BirSign:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["BirSign:ClientSecret"],
                RedirectUri = ConfigurationManager.AppSettings["BirSign:RedirectUri"],
                PostLogoutRedirectUri = ConfigurationManager.AppSettings["BirSign:PostLogoutRedirectUri"],
                Scope = OpenIdConnectScope.OpenIdProfile,
                AuthenticationType = BirSignConstants.AuthenticationType,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                ResponseType = OpenIdConnectResponseType.Code,
                ResponseMode = OpenIdConnectResponseMode.Query,
                UsePkce = true,
                RedeemCode = true,
                UseTokenLifetime = false,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "name",
                    RoleClaimType = "role",
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async notification =>
                    {
                        var identity = notification.AuthenticationTicket.Identity;
                        identity.AddUserRoles();

                        var sub = identity.FindFirst("sub")?.Value 
                                  ?? identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (!string.IsNullOrEmpty(sub))
                        {
                            MemoryCache.Default.Remove($"RevokedUser_{sub}");
                        }

                        if (manageUser != null)
                        {
                            await manageUser(identity);
                        }
                    },
                    RedirectToIdentityProvider = async notification =>
                    {
                        var openIdConnectRequestType = notification.ProtocolMessage.RequestType;
                        if (openIdConnectRequestType == OpenIdConnectRequestType.Authentication)
                        {
                            await ParHelper.EnablePar(notification);
                        }
                    },
                },
            };
        }
    }
}
