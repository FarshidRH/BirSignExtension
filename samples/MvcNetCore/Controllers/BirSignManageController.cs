using MapIdeaHub.BirSign.SharedKernel.Dtos;
using MapIdeaHub.BirSign.SharedKernel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MvcNetCore.Controllers
{
    public class BirSignManageController(
        IdsService idsService,
        RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly IdsService _idsService = idsService;
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
            var userRequest = new UserRequest
            {
                NationalCode = "1234567890",
                BirthDate = "1234/56/78",
                PhoneNumber = "091234567890",
                Email = "user@example.com",
                ActiveDirectoryUser = null,
            };

            var result = await _idsService.SendUsersAsync(userRequest);
            return Content(result);
        }
    }
}