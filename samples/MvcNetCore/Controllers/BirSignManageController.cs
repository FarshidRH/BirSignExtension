using MapIdeaHub.BirSign.SharedKernel.Dtos;
using MapIdeaHub.BirSign.SharedKernel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MvcNetCore.Controllers
{
    public class BirSignManageController(
        IdsService _idsService,
        RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public async Task<ActionResult> SendRoles()
        {
            var roles = await _roleManager.Roles
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
            return Json(result);
        }

        public async Task<ActionResult> SendUsers()
        {
            var user = new UserRequest
            {

            };

            var result = await _idsService.SendUsersAsync(user);
            return Json(result);
        }
    }
}