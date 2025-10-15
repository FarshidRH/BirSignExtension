using MapIdeaHub.BirSign.NetFrameworkExtension.Events;
using Microsoft.IdentityModel.Tokens;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Integration.Options
{
    public class AuthenticationOptions
    {
        public string ClientId { get; set; }

        public string Authority { get; set; }

        public string RedirectUri { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public string ClientSecret { get; set; }

        public string ResponseType { get; set; } = "id_token";

        public string AuthenticationType { get; set; }

        public string Scope { get; set; } = "openid profile";

        public bool UseTokenLifetime { get; set; } = false;

        public bool RequireHttpsMetadata { get; set; } = false;

        public bool SaveTokens { get; set; } = true;

        public TokenValidationParameters TokenValidationParameters { get; set; } = new TokenValidationParameters();

        public CustomeEvents Events { get; set; } = new CustomeEvents();
    }
}
