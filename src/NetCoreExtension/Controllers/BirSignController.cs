using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
    [AllowAnonymous]
    public async Task<ActionResult> BackChannelLogout([FromServices] IMemoryCache memoryCache)
    {
        try
        {
            var form = await Request.ReadFormAsync();
            var logoutToken = form["logout_token"].ToString();
            if (string.IsNullOrEmpty(logoutToken))
            {
                return BadRequest("Logout token is missing.");
            }

            var principal = await ValidateLogoutTokenAsync(logoutToken);

            var sub = principal.FindFirst("sub")?.Value 
                      ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(sub))
            {
                // Record the revoked subject ID in memory cache for 24 hours
                memoryCache.Set($"RevokedUser_{sub}", true, TimeSpan.FromHours(24));
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Back-channel logout validation failed: {ex.Message}");
        }
    }

    private static async Task<ClaimsPrincipal> ValidateLogoutTokenAsync(string logoutToken)
    {
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{BirSignSettings.Authority}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());

        var config = await configManager.GetConfigurationAsync(CancellationToken.None);
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParams = new TokenValidationParameters
        {
            ValidIssuer = BirSignSettings.Authority,
            ValidateAudience = false,
            IssuerSigningKeys = config.SigningKeys,
            ValidateLifetime = true
        };

        var principal = tokenHandler.ValidateToken(logoutToken, validationParams, out _);
        return principal;
    }
}
