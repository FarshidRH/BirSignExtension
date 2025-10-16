using MapIdeaHub.BirSign.NetFrameworkExtension.Constants;
using MapIdeaHub.BirSign.NetFrameworkExtension.Events;
using Microsoft.IdentityModel.Tokens;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Options
{
    public class BirSignAuthenticationOptions
    {
        public BirSignAuthenticationOptions(
            string clientId,
            string clientSecret,
            string authority,
            string redirectUri,
            string postLogoutRedirectUri,
            string authenticationType = BirSignConstants.AuthenticationType,
            string responseType = BirSignConstants.ResponseType,
            string scope = BirSignConstants.Scope,
            bool useTokenLifetime = false,
            bool requireHttpsMetadata = false,
            bool saveTokens = true,
            IdsEvents events = null,
            TokenValidationParameters tokenValidationParameters = null)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Authority = authority;
            RedirectUri = redirectUri;
            PostLogoutRedirectUri = postLogoutRedirectUri;
            AuthenticationType = authenticationType;
            ResponseType = responseType;
            Scope = scope;
            UseTokenLifetime = useTokenLifetime;
            RequireHttpsMetadata = requireHttpsMetadata;
            SaveTokens = saveTokens;
            Events = events;

            TokenValidationParameters = tokenValidationParameters ?? new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
            };
        }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Authority { get; set; }

        public string RedirectUri { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public string AuthenticationType { get; set; }

        public string ResponseType { get; set; }

        public string Scope { get; set; }

        public bool UseTokenLifetime { get; set; }

        public bool RequireHttpsMetadata { get; set; }

        public bool SaveTokens { get; set; }

        public IdsEvents Events { get; set; }

        public TokenValidationParameters TokenValidationParameters { get; set; }
    }
}
