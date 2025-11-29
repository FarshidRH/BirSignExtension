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

namespace MapIdeaHub.BirSign.NetFrameworkExtension
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Enables BirSign authentication using default configuration settings in the OWIN pipeline.
        /// </summary>
        /// <remarks>This extension method configures BirSign authentication with default options. To
        /// customize authentication behavior, use the overload that accepts configuration options.</remarks>
        /// <param name="app">The OWIN application builder to which the BirSign authentication middleware will be added.</param>
        /// <returns>The original <see cref="IAppBuilder"/> instance, enabling further configuration of the OWIN pipeline.</returns>
        public static IAppBuilder UseBirSignAuthentication(this IAppBuilder app)
        {
            var options = GetDefaultOpenIdConnectAuthenticationOptions(null);
            return app.UseBirSignAuthentication(options);
        }

        /// <summary>
        /// Enables BirSign authentication using OpenID Connect in the OWIN application pipeline.
        /// </summary>
        /// <remarks>Call this method to add BirSign authentication to your OWIN pipeline. The method
        /// applies default OpenID Connect options, then allows further customization through the provided configurator
        /// action. This extension should be called during application startup.</remarks>
        /// <param name="app">The OWIN application builder to which the authentication middleware is added.</param>
        /// <param name="manageUser">A delegate that is invoked to manage the authenticated user's claims identity after successful
        /// authentication. If null is provided, no additional user management is performed.</param>
        /// <param name="optionsConfigurator">An action to configure additional OpenID Connect authentication options before the middleware is registered.
        /// If null is provided, no additional configuration is applied.</param>
        /// <returns>The original <see cref="IAppBuilder"/> instance, enabling further middleware configuration.</returns>
        public static IAppBuilder UseBirSignAuthentication(this IAppBuilder app,
            Func<ClaimsIdentity, Task> manageUser,
            Action<OpenIdConnectAuthenticationOptions> optionsConfigurator)
        {
            var options = GetDefaultOpenIdConnectAuthenticationOptions(manageUser);

            if (optionsConfigurator != null)
            {
                optionsConfigurator(options);
            }

            return app.UseBirSignAuthentication(options);
        }

        private static IAppBuilder UseBirSignAuthentication(this IAppBuilder app,
            OpenIdConnectAuthenticationOptions options)
        {
            BirSignSettings.Authority = options.Authority;
            BirSignSettings.RegisterUri = $"{options.Authority.TrimEnd('/')}/Account/Register";

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
                ResponseType = OpenIdConnectResponseType.IdToken,
                //ResponseMode = OpenIdConnectResponseMode.Query,
                Scope = OpenIdConnectScope.OpenIdProfile,
                AuthenticationType = BirSignConstants.AuthenticationType,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                UseTokenLifetime = false,
                //UsePkce = true,
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
                    SecurityTokenValidated = async n =>
                    {
                        var identity = n.AuthenticationTicket.Identity;
                        identity.AddUserRoles();

                        if (manageUser != null)
                        {
                            await manageUser(identity);
                        }
                    },
                },
            };
        }
    }
}
