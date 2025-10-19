using MapIdeaHub.BirSign.NetFrameworkExtension.Constants;
using MapIdeaHub.BirSign.NetFrameworkExtension.Options;
using Microsoft.Owin.Security;
using System.Web;
using System.Web.Mvc;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Controllers
{
    public class BirSignController : Controller
    {
        //
        // GET: /BirSign/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = BirSignAuthenticationOptions.StaticRedirectUri,
            };
            HttpContext.GetOwinContext().Authentication.Challenge(properties, BirSignConstants.AuthenticationType);
            return new HttpUnauthorizedResult();
        }
    }
}
