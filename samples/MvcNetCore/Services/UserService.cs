using Microsoft.AspNetCore.Identity;
using MvcNetCore.Models;
using System.Security.Claims;

namespace MvcNetCore.Services
{
    public class UserService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        public async Task EnsureUserExistsAsync(ClaimsIdentity identity)
        {
            ApplicationUser? user = await _userManager.FindByNameAsync(identity.Name!);

            if (user is null)
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
                await _userManager.CreateAsync(user);
            }
        }

        public async Task ManageUserRolesAsync(ClaimsIdentity identity)
        {
            var ssoRoles = identity.Claims
                .Where(c => c.Type.StartsWith("MPI_"))
                .Select(c => c.Value)
                .ToArray();

            foreach (var role in ssoRoles)
            {
                identity.AddClaim(new Claim("role", role));
            }
        }
    }
}