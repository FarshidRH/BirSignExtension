using MapIdeaHub.BirSign.NetFrameworkExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;

namespace MapIdeaHub.BirSign.NetFrameworkExtension
{
    public static class AuthenticationExtensions
    {
        public static IAppBuilder UseBirSignAuthentication(this IAppBuilder app)
        {
            var options = GetDefaultOpenIdConnectAuthenticationOptions();
            return app.UseBirSignAuthentication(options);
        }

        public static IAppBuilder UseBirSignAuthentication(this IAppBuilder app,
            Action<OpenIdConnectAuthenticationOptions> optionsConfigurator)
        {
            var options = GetDefaultOpenIdConnectAuthenticationOptions();
            optionsConfigurator(options);
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

        private static OpenIdConnectAuthenticationOptions GetDefaultOpenIdConnectAuthenticationOptions()
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
                Notifications = new OpenIdConnectAuthenticationNotifications(),
            };
        }
    }
}
