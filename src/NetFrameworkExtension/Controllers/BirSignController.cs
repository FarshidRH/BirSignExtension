using MapIdeaHub.BirSign.NetFrameworkExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Controllers
{
    public class BirSignController : Controller
    {
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        [AllowAnonymous]
        public void Login(string returnUrl)
        {
            var properties = new AuthenticationProperties { RedirectUri = returnUrl ?? "/" };
            AuthenticationManager.Challenge(properties, BirSignConstants.AuthenticationType);
        }

        [HttpGet]
        public ActionResult FrontChannelLogout()
        {
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie,
                DefaultAuthenticationTypes.ExternalCookie,
                BirSignConstants.AuthenticationType);

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> BackChannelLogout()
        {
            var logoutToken = Request.Form["logout_token"];
            if (string.IsNullOrEmpty(logoutToken))
            {
                return new HttpStatusCodeResult(400, "Logout token is missing.");
            }

            try
            {
                var principal = await ValidateLogoutTokenAsync(logoutToken);

                var sub = principal.FindFirst("sub")?.Value 
                          ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(sub))
                {
                    MemoryCache.Default.Set(
                        $"RevokedUser_{sub}",
                        true,
                        new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(24) });
                }

                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(400, $"Logout token is invalid: {ex.Message}");
            }
        }

        private static async Task<ClaimsPrincipal> ValidateLogoutTokenAsync(string logoutToken)
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{BirSignSettings.Authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            var config = await configManager.GetConfigurationAsync(CancellationToken.None);
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidIssuer = BirSignSettings.Authority,
                ValidateAudience = false,
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true
            };

            var principal = tokenHandler.ValidateToken(logoutToken, validationParams, out _);
            return principal;
        }
    }
}
