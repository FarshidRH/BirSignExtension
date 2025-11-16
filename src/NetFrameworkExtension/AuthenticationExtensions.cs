using MapIdeaHub.BirSign.NetFrameworkExtension.Constants;
using MapIdeaHub.BirSign.NetFrameworkExtension.Enums;
using MapIdeaHub.BirSign.NetFrameworkExtension.Utilities;
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
            var authority = options.Authority.TrimEnd('/');
            BirSignConstants.Authority = authority;
            BirSignConstants.RegisterUri = $"{authority}/Account/Register";

            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ApplicationCookie);
            return app.UseOpenIdConnectAuthentication(options);
        }

        private static OpenIdConnectAuthenticationOptions GetDefaultOpenIdConnectAuthenticationOptions()
        {
            return new OpenIdConnectAuthenticationOptions
            {
                Authority = ConfigurationManager.AppSettings["BirSign:Authority"],
                ClientId = ConfigurationManager.AppSettings["BirSign:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["BirSign:ClientSecret"].ComputeHash(HashType.SHA256),
                RedirectUri = ConfigurationManager.AppSettings["BirSign:RedirectUri"],
                PostLogoutRedirectUri = ConfigurationManager.AppSettings["BirSign:PostLogoutRedirectUri"],
                ResponseType = OpenIdConnectResponseType.IdToken,
                //ResponseMode = OpenIdConnectResponseMode.Query,
                Scope = OpenIdConnectScope.OpenIdProfile,
                AuthenticationType = BirSignConstants.AuthenticationType,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                SaveTokens = true,
                UseTokenLifetime = false,
                //UsePkce = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                },
            };
        }
    }
}
