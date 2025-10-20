using MapIdeaHub.BirSign.NetFrameworkExtension.Constants;
using MapIdeaHub.BirSign.NetFrameworkExtension.Enums;
using MapIdeaHub.BirSign.NetFrameworkExtension.Events;
using MapIdeaHub.BirSign.NetFrameworkExtension.Utilities;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Options
{
    public class BirSignAuthenticationOptions
    {
        internal BirSignAuthenticationOptions()
        {
            ClientId = ConfigurationManager.AppSettings["BirSignClientId"];
            ClientSecret = ConfigurationManager.AppSettings["BirSignClientSecret"].ComputeHash(HashType.SHA256);
            Authority = ConfigurationManager.AppSettings["BirSignIdsUri"];
            RedirectUri = ConfigurationManager.AppSettings["BirSignRedirectUri"];
            PostLogoutRedirectUri = ConfigurationManager.AppSettings["BirSignPostLogoutRedirectUri"];
            AuthenticationType = BirSignConstants.AuthenticationType;
            ResponseType = BirSignConstants.ResponseType;
            Scope = BirSignConstants.Scope;
            UseTokenLifetime = false;
            RequireHttpsMetadata = false;
            SaveTokens = true;
            TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
            };
            Events = null;
            StaticRedirectUri = RedirectUri;
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

        public TokenValidationParameters TokenValidationParameters { get; set; }

        public IdsEvents Events { get; set; }

        internal static string StaticRedirectUri;
    }
}
