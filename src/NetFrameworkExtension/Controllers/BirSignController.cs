using MapIdeaHub.BirSign.NetFrameworkExtension.Constants;
using MapIdeaHub.BirSign.NetFrameworkExtension.Options;
using MapIdeaHub.BirSign.NetFrameworkExtension.Services;
using Microsoft.Owin.Security;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Controllers
{
    public class BirSignController : Controller
    {
        private readonly IdsService _idsService;

        private IAuthenticationManager AuthenticationManager
            => HttpContext.GetOwinContext().Authentication;

        public BirSignController()
        {
            _idsService = new IdsService();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = BirSignAuthenticationOptions._staticRedirectUri,
            };
            HttpContext.GetOwinContext().Authentication.Challenge(properties, BirSignConstants.AuthenticationType);
            return new HttpUnauthorizedResult();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Logout()
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
                    string birSignIdsUri = ConfigurationManager.AppSettings["BirSignIdsUri"];
                    var logoutUri = $"{birSignIdsUri}/api/logout/process";
                    await _idsService.LogoutAsync(logoutToken, logoutUri);
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(500, $"Logout failed: {ex.Message}");
                }

                AuthenticationManager.SignOut(BirSignConstants.AuthenticationType);
                Session.Abandon();
            }

            return new HttpStatusCodeResult(200);
        }
    }
}
