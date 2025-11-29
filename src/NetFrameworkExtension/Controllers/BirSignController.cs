using MapIdeaHub.BirSign.NetFrameworkExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using System.IdentityModel.Tokens.Jwt;
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
        public async Task<ActionResult> BackChannelLogout()
        {
            if (User.Identity.IsAuthenticated)
            {
                var logoutToken = Request.Form["logout_token"];
                if (string.IsNullOrEmpty(logoutToken))
                {
                    return new HttpStatusCodeResult(400, "Logout token is missing.");
                }

                try
                {
                    await ValidateLogoutTokenAsync(logoutToken);

                    AuthenticationManager.SignOut(
                        DefaultAuthenticationTypes.ApplicationCookie,
                        DefaultAuthenticationTypes.ExternalCookie,
                        BirSignConstants.AuthenticationType);
                }
                catch
                {
                    return new HttpStatusCodeResult(400, "Logout token is invalid.");
                }
            }

            return new HttpStatusCodeResult(200);
        }

        private async Task ValidateLogoutTokenAsync(string logoutToken)
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{BirSignSettings.Authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            var config = await configManager.GetConfigurationAsync(CancellationToken.None);
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidIssuer = BirSignSettings.Authority,
                ValidateAudience = false, // logout_token has no audience claim
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true
            };

            tokenHandler.ValidateToken(logoutToken, validationParams, out _);
        }
    }
}
