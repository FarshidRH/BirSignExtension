using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using MapIdeaHub.BirSign.NetFrameworkExtension.Integration.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MapIdeaHub.BirSign.NetFrameworkExtension
{
    public static class AuthenticationExtensions
    {
        public static IAppBuilder UseBirSignAuthentication(this IAppBuilder app, AuthenticationOptions options)
        {
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = options.AuthenticationType,
                ClientId = options.ClientId,
                Authority = options.Authority,
                RedirectUri = options.RedirectUri,
                PostLogoutRedirectUri = options.PostLogoutRedirectUri,
                ClientSecret = options.ClientSecret,
                ResponseType = options.ResponseType,
                Scope = options.Scope,
                UseTokenLifetime = options.UseTokenLifetime,
                RequireHttpsMetadata = options.RequireHttpsMetadata,
                SaveTokens = options.SaveTokens,
                TokenValidationParameters = options.TokenValidationParameters,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async (context) =>
                    {
                        var token = context.ProtocolMessage.IdToken;
                        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                              context.Options.Authority + "/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                        var openIdConfig = await configurationManager.GetConfigurationAsync();
                        var jwks = openIdConfig.JsonWebKeySet;
                        var signingKeys = jwks.Keys.Select(jwk =>
                        {
                            var rsaParameters = new RSAParameters
                            {
                                Modulus = Base64UrlDecode(jwk.N),
                                Exponent = Base64UrlDecode(jwk.E)
                            };
                            return new RsaSecurityKey(rsaParameters);
                        }).ToList();
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var validationParameters = new TokenValidationParameters
                        {
                            ValidIssuer = context.Options.Authority,
                            ValidAudience = options.ClientId,
                            IssuerSigningKeys = signingKeys
                        };
                        try
                        {
                            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                            var identity = context.AuthenticationTicket.Identity;
                            var userInfo = ExtractUserInfo(identity);
                            userInfo.Identity = identity;
                            bool userExists = options.Events?.OnCheckUserExists != null && await options.Events.OnCheckUserExists(userInfo);
                            if (!userExists)
                            {
                                if (options.Events?.OnUserRegistered != null)
                                    await options.Events.OnUserRegistered(userInfo);
                            }
                            if (options.Events?.OnManageUserAccess != null)
                                await options.Events.OnManageUserAccess(userInfo);
                            if (options.Events?.OnUserAuthenticated != null)
                                await options.Events.OnUserAuthenticated(userInfo);
                        }
                        catch (SecurityTokenException)
                        {

                        }
                    }
                }
            });

            return app;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string padded = input.Length % 4 == 0 ? input : input + new string('=', 4 - input.Length % 4);
            string base64 = padded.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(base64);
        }

        private static UserInfo ExtractUserInfo(ClaimsIdentity identity) => new UserInfo
        {
            Name = identity.Claims.FirstOrDefault(c => c.Type == "MPH_name")?.Value ?? "",
            Family = identity.Claims.FirstOrDefault(c => c.Type == "MPH_family")?.Value ?? "",
            NationalCode = identity.Claims.FirstOrDefault(c => c.Type == "MPH_national")?.Value ?? "",
            BirthDate = identity.Claims.FirstOrDefault(c => c.Type == "MPH_birthdate")?.Value ?? "",
            Gender = identity.Claims.FirstOrDefault(c => c.Type == "MPH_gender")?.Value ?? "",
            Email = identity.Claims.FirstOrDefault(c => c.Type == "MPH_email")?.Value ?? "",
            PhoneNumber = identity.Claims.FirstOrDefault(c => c.Type == "MPH_phonenumber")?.Value ?? "",
            Roles = identity.Claims.Where(c => c.Type.StartsWith("MPI_")).Select(c => c.Value).ToList(),
            Claims = identity.Claims.ToDictionary(c => c.Type, c => c.Value)
        };
    }
}
