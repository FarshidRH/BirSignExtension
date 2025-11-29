using Microsoft.AspNetCore.Identity;
using MvcNetCore.Models;
using System.Security.Claims;

namespace MvcNetCore.Helpers
{
    public class UserHelper
    {
        public static async Task EnsureUserExistsAsync(
            IServiceProvider serviceProvider, ClaimsIdentity identity)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            ApplicationUser? user = await userManager.FindByNameAsync(identity.Name!);

            if (user is null)
            {
                user = new()
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