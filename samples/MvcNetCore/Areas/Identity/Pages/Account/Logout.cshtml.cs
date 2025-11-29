// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MvcNetCore.Models;

namespace MvcNetCore.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            if (BirSignSettings.IsUseBirSign(_configuration))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return SignOut(
                    new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") },
                    BirSignConstants.AuthenticationType);
            }
            else
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");

                if (returnUrl != null)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    // This needs to be a redirect so that the browser performs a new
                    // request and the identity for the user gets updated.
                    return RedirectToPage();
                }
            }
        }
    }
}
