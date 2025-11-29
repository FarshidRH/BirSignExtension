using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MapIdeaHub.BirSign.NetCoreExtension.Controllers;

public class BirSignController : Controller
{
    [AllowAnonymous]
    public async Task Login(string returnUrl)
    {
        var properties = new AuthenticationProperties { RedirectUri = returnUrl ?? "/" };
        await HttpContext.ChallengeAsync(BirSignConstants.AuthenticationType, properties);
    }

    [HttpPost]
    public async Task<ActionResult> BackChannelLogout()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var form = await Request.ReadFormAsync();
            var logoutToken = form["logout_token"].ToString();
            if (string.IsNullOrEmpty(logoutToken))
            {
                return BadRequest();
            }

            try
            {
                await ValidateLogoutTokenAsync(logoutToken!);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(BirSignConstants.AuthenticationType);
            }
            catch
            {
                return BadRequest();
            }
        }

        return Ok();
    }

    private static async Task ValidateLogoutTokenAsync(string logoutToken)
    {
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{BirSignSettings.Authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());

        var config = await configManager.GetConfigurationAsync(CancellationToken.None);
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParams = new TokenValidationParameters
        {
            ValidIssuer = BirSignSettings.Authority,
            ValidateAudience = false, // logout_token has no audience claim
            IssuerSigningKeys = config.SigningKeys,
            ValidateLifetime = true
        };

        tokenHandler.ValidateToken(logoutToken, validationParams, out _);
    }
}
