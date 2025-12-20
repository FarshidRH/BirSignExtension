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
        /// <remarks>Call this method to add BirSign authentication to your OWIN pipeline. The method
        /// applies default OpenID Connect options, then allows further customization through the provided configurator
        /// action. This extension should be called during application startup.</remarks>
        /// <param name="app">The OWIN application builder to which the authentication middleware is added.</param>
        /// <param name="manageUser">An optional function to manage user creation or updates upon successful authentication.</param>
        /// <param name="optionsConfigurator">An optional action to further customize the OpenID Connect authentication options.</param>
        /// <returns>The original <see cref="IAppBuilder"/> instance, enabling further middleware configuration.</returns>
        public static IAppBuilder UseBirSignAuthentication(
            this IAppBuilder app,
            Func<ClaimsIdentity, Task> manageUser = null,
            Action<OpenIdConnectAuthenticationOptions> optionsConfigurator = null)
        {
            var options = GetDefaultOpenIdConnectAuthenticationOptions(manageUser);
            optionsConfigurator?.Invoke(options);

            BirSignSettings.Authority = options.Authority;
            BirSignSettings.RegisterUri = $"{options.Authority.TrimEnd('/')}/Account/Register";

            AntiForgeryConfig.UniqueClaimTypeIdentifier = options.TokenValidationParameters.NameClaimType;
            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ApplicationCookie);
            return app.UseOpenIdConnectAuthentication(options);
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
