using MapIdeaHub.BirSign.NetFrameworkExtension.Constants;
using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MvcNetFramework.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MvcNetFramework.Helpers
{
    public static class IdsHelper
    {
        private static ApplicationUserManager UserManager
            => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

        private static IAuthenticationManager AuthenticationManager
            => HttpContext.Current.GetOwinContext().Authentication;

        public static async Task<bool> OnCheckUserExists(UserInfo userInfo)
            => await UserManager.FindByNameAsync(userInfo.NationalCode) != null;

        public static async Task OnUserRegistered(UserInfo userInfo)
        {
            var user = await UserManager.FindByNameAsync(userInfo.NationalCode);
            if (user != null)
            {
                return;
            }

            user = new ApplicationUser()
            {
                Name = userInfo.Name,
                Family = userInfo.Family,
                NationalCode = userInfo.NationalCode,
                BirthDay = userInfo.BirthDate,
                Email = userInfo.Email,
                PhoneNumber = userInfo.PhoneNumber,
                UserName = userInfo.NationalCode
            };
            UserManager.Create(user);
            UserManager.AddPassword(user.Id, Guid.NewGuid().ToString());
        }

        public static async Task OnManageUserAccess(UserInfo userInfo)
        {
            var user = await UserManager.FindByNameAsync(userInfo.NationalCode);
            if (user == null)
            {
                return;
            }

            var dbUserRoles = await UserManager.GetRolesAsync(user.Id);
            await UserManager.RemoveFromRolesAsync(user.Id, dbUserRoles.ToArray());

            var ssoUserRoles = userInfo.Roles;
            await UserManager.AddToRolesAsync(user.Id, ssoUserRoles.ToArray());
        }

        public static async Task OnUserAuthenticated(UserInfo userInfo)
        {
            var user = await UserManager.FindByNameAsync(userInfo.NationalCode);
            if (user == null)
            {
                return;
            }

            var userIdentity = UserManager.CreateIdentity(user, BirSignConstants.AuthenticationType);
            userIdentity.AddClaims(userInfo.Identity.Claims.Where(p => p.Type == "sid"));

            var authenticationTypes = new string[]
            {
                DefaultAuthenticationTypes.ApplicationCookie,
                DefaultAuthenticationTypes.ExternalCookie,
                DefaultAuthenticationTypes.TwoFactorCookie,
                DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie,
                BirSignConstants.AuthenticationType,
            };
            AuthenticationManager.SignOut(authenticationTypes);

            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15),
            };
            AuthenticationManager.SignIn(authenticationProperties, userIdentity);
        }
    }
}