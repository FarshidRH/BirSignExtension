using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using MapIdeaHub.BirSign.NetFrameworkExtension.Services;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcNetFramework.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BirSignManageController : Controller
    {
        private readonly IdsService _idsService;
        private ApplicationRoleManager _roleManager;

        public ApplicationRoleManager RoleManager
        {
            get => _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            private set => _roleManager = value;
        }

        public BirSignManageController()
        {
            _idsService = new IdsService();
        }

        public BirSignManageController(
            ApplicationRoleManager roleManager) : this()
        {
            RoleManager = roleManager;
        }

        public async Task<ActionResult> SendRoles()
        {
            var roles = await RoleManager.Roles
                .AsNoTracking()
                .Select(x => new RoleInfo
                {
                    Name = x.Name,
                    SourcePrimaryKey = x.Id,
                })
                .ToListAsync();

            foreach (var role in roles)
            {
                switch (role.Name)
                {
                    case "Admin":
                        role.IsPublicForAll = false;
                        role.IsPublicForOrganUsers = false;
                        role.Description = "دسترسی ادمین برای مدیریت کامل سیستم";
                        break;
                    case "User":
                        role.IsPublicForAll = true;
                        role.IsPublicForOrganUsers = false;
                        role.Description = "دسترسی عمومی برای کاربران عادی سامانه";
                        break;
                    default:
                        throw new NotImplementedException($"The role of '{role.Name}' is not defined in the system.");
                }
            }

            var result = await _idsService.SendRolesAsync(new RoleRequest { Roles = roles });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SendUsers()
        {
            var user = new UserRequest
            {

            };

            var result = await _idsService.SendUsersAsync(user);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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