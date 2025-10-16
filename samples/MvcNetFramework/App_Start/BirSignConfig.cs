using MapIdeaHub.BirSign.NetFrameworkExtension;
using MapIdeaHub.BirSign.NetFrameworkExtension.Dtos;
using MapIdeaHub.BirSign.NetFrameworkExtension.Events;
using MapIdeaHub.BirSign.NetFrameworkExtension.Options;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using MvcNetFramework.Constants;
using MvcNetFramework.Models;
using Owin;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MvcNetFramework
{
    public static class BirSignConfig
    {
        private static ApplicationUserManager UserManager
            => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

        private static IAuthenticationManager AuthenticationManager
            => HttpContext.Current.GetOwinContext().Authentication;

        public static IAppBuilder UseBirSignAuthentication(this IAppBuilder app)
        {
            return app.UseBirSignAuthentication(new BirSignAuthenticationOptions()
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                ClientId = IdsConstants.IdsClientId,
                Authority = IdsConstants.IdsServerUrl,
                RedirectUri = "https://localhost:44331/Home/Index",
                PostLogoutRedirectUri = "https://localhost:44331/Home/Index",
                ClientSecret = IdsConstants.IdsClientSecretNotHashed,
                ResponseType = "id_token",
                Scope = "openid profile",
                UseTokenLifetime = false,
                RequireHttpsMetadata = false,
                SaveTokens = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                },
                Events = new IdsEvents
                {
                    OnCheckUserExists = OnCheckUserExists,
                    OnUserRegistered = OnUserRegistered,
                    OnManageUserAccess = OnManageUserAccess,
                    OnUserAuthenticated = OnUserAuthenticated,
                }
            });
        }

        private static async Task<bool> OnCheckUserExists(UserInfo userInfo)
            => await UserManager.FindByNameAsync(userInfo.NationalCode) != null;

        private static async Task OnUserRegistered(UserInfo userInfo)
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
            UserManager.AddPassword(user.Id, "$trongP@ssW0rd");
        }

        private static async Task OnManageUserAccess(UserInfo userInfo)
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

        private static async Task OnUserAuthenticated(UserInfo userInfo)
        {
            var user = await UserManager.FindByNameAsync(userInfo.NationalCode);
            if (user == null)
            {
                return;
            }

            var userIdentity = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            userIdentity.AddClaims(userInfo.Identity.Claims.Where(p => p.Type == "sid"));

            var authenticationTypes = new string[]
            {
                DefaultAuthenticationTypes.ApplicationCookie,
                DefaultAuthenticationTypes.ExternalCookie
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