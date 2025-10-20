using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using MapIdeaHub.BirSign.NetFrameworkExtension.Services;
using Microsoft.AspNet.Identity.Owin;
using MvcNetFramework.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcNetFramework.Controllers
{
    public class HomeController : Controller
    {
        private readonly IdsService _idsService;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public HomeController()
        {
            _idsService = new IdsService();
        }

        public HomeController(
            ApplicationUserManager userManager,
            ApplicationRoleManager roleManager) : this()
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ApplicationRoleManager RoleManager
        {
            get => _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            private set => _roleManager = value;
        }

        public async Task<ActionResult> Index()
        {
            ApplicationUser user = null;

            if (User.Identity.IsAuthenticated)
            {
                user = await UserManager.FindByNameAsync(User.Identity.Name);
                ViewBag.Roles = await UserManager.GetRolesAsync(user.Id);
            }

            return View(user);
        }

        [Authorize]
        public async Task<ActionResult> SendRoles()
        {
            var roles = await RoleManager.Roles
                .AsNoTracking()
                .Select(x => new RoleInfo
                {
                    Name = x.Name,
                    SourcePrimaryKey = x.Id,
                    IsPublicForAll = x.IsPublicAccess,
                    IsPublicForOrganUsers = x.IsPersonnelAccess,
                    Description = x.Description,
                })
                .ToListAsync();

            var result = await _idsService.SendRolesAsync(new RoleRequest { Roles = roles });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }

                base.Dispose(disposing);
            }
        }
    }
}