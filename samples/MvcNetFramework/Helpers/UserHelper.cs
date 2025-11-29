using Microsoft.AspNet.Identity.Owin;
using MvcNetFramework.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace MvcNetFramework.Services
{
    public class UserHelper
    {
        public static async Task EnsureUserExistsAsync(ClaimsIdentity identity)
        {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = await userManager.FindByNameAsync(identity.Name);

            if (user == null)
            {
                user = new ApplicationUser()
                {
                    Name = identity.Claims.FirstOrDefault(c => c.Type == "MPH_name")?.Value ?? "",
                    Family = identity.Claims.FirstOrDefault(c => c.Type == "MPH_family")?.Value ?? "",
                    BirthDay = identity.Claims.FirstOrDefault(c => c.Type == "MPH_birthdate")?.Value ?? "",
                    Email = identity.Claims.FirstOrDefault(c => c.Type == "MPH_email")?.Value ?? "",
                    PhoneNumber = identity.Claims.FirstOrDefault(c => c.Type == "MPH_phonenumber")?.Value ?? "",
                    NationalCode = identity.Name,
                    UserName = identity.Name
                };
                await userManager.CreateAsync(user);
            }
        }
    }
}