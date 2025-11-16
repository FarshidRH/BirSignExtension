using Microsoft.AspNet.Identity.Owin;
using MvcNetFramework.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MvcNetFramework.Services
{
    public class UserService
    {
        private static ApplicationUserManager UserManager
          => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

        public async Task EnsureUserExistsAsync(ClaimsIdentity identity)
        {
            if (await UserManager.FindByNameAsync(identity.Name) == null)
            {
                var user = new ApplicationUser()
                {
                    Name = identity.Claims.FirstOrDefault(c => c.Type == "MPH_name")?.Value ?? "",
                    Family = identity.Claims.FirstOrDefault(c => c.Type == "MPH_family")?.Value ?? "",
                    BirthDay = identity.Claims.FirstOrDefault(c => c.Type == "MPH_birthdate")?.Value ?? "",
                    Email = identity.Claims.FirstOrDefault(c => c.Type == "MPH_email")?.Value ?? "",
                    PhoneNumber = identity.Claims.FirstOrDefault(c => c.Type == "MPH_phonenumber")?.Value ?? "",
                    NationalCode = identity.Name,
                    UserName = identity.Name
                };
                await UserManager.CreateAsync(user);
            }
        }

        public async Task ManageUserRolesAsync(ClaimsIdentity identity)
        {
            var user = await UserManager.FindByNameAsync(identity.Name);
            if (user == null) { return; }

            var dbUserRoles = (await UserManager.GetRolesAsync(user.Id)).ToArray();
            await UserManager.RemoveFromRolesAsync(user.Id, dbUserRoles);

            var ssoUserRoles = identity.Claims.Where(c => c.Type.StartsWith("MPI_")).Select(c => c.Value).ToArray();
            await UserManager.AddToRolesAsync(user.Id, ssoUserRoles);
        }
    }
}