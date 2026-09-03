using MapIdeaHub.BirSign.NetCoreExtension.Models;
#if !NET9_0_OR_GREATER
using MapIdeaHub.BirSign.NetCoreExtension.Helpers;
#endif
using MapIdeaHub.BirSign.SharedKernel.Constants;
using MapIdeaHub.BirSign.SharedKernel.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace MapIdeaHub.BirSign.NetCoreExtension;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds BirSign configuration as the IdP.
    /// This method configures BirSign authentication, replacing or augmenting existing ASP.NET Core Identity.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The IConfiguration instance containing BirSign settings.</param>
    /// <param name="manageUser">Optional function to manage user creation or updates upon successful authentication.</param>
    /// <param name="optionsConfigurator">Optional action to further customize OpenIdConnectOptions.</param>
    /// <param name="signInScheme">
    /// The scheme the OpenID Connect handler signs into. Leave null — the default — and this
    /// method owns the session: it adds its own cookie scheme, points the application's default
    /// schemes at it, and handles back-channel logout there.
    /// <para>
    /// Pass a scheme instead when the application already owns its sign-in cookie, as anything
    /// built on ASP.NET Core Identity does (<c>IdentityConstants.ExternalScheme</c> is the usual
    /// value). This method then adds nothing but the OpenID Connect handler and leaves the
    /// default schemes alone, because repointing them would send every <c>[Authorize]</c> in the
    /// application to the wrong cookie. Completing the sign-in and invalidating a session on
    /// back-channel logout become the application's job.
    /// </para>
    /// </param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddBirSignAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, ClaimsIdentity, Task>? manageUser = null,
        Action<OpenIdConnectOptions>? optionsConfigurator = null,
        string? webhookUrl = "/api/birsign/webhook",
        Func<IServiceProvider, MapIdeaHub.BirSign.SharedKernel.Dtos.WebhookEvent, Task>? webhookHandler = null,
        string? signInScheme = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Load BirSign settings from configuration
        var ssoConfig = configuration.GetSection("BirSign");
        var authority = ssoConfig["Authority"];
        var clientId = ssoConfig["ClientId"];
        var clientSecret = ssoConfig["ClientSecret"];
        var redirectUri = ssoConfig["RedirectUri"];
        var logoutRedirectUri = ssoConfig["LogoutRedirectUri"];
        var postLogoutRedirectUri = ssoConfig["PostLogoutRedirectUri"];

        // Publish the register/forgot-password/manage links now rather than from the options
        // callback below. That callback only runs when the options are first resolved, which is
        // the first challenge — long after a login page needs to render the links.
        BirSignSettings.SetAuthority(authority);

        services.AddMemoryCache();

        AuthenticationBuilder authBuilder;

        if (signInScheme is null)
        {
            // Add authentication services
            authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = BirSignConstants.AuthenticationType;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            // Add cookie authentication for session management with back-channel logout check
            authBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Events.OnValidatePrincipal = async (context) =>
                {
                    var sub = context.Principal?.FindFirst("sub")?.Value
                              ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(sub))
                    {
                        var memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
                        if (memoryCache != null && memoryCache.TryGetValue($"RevokedUser_{sub}", out _))
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        }
                    }
                };
            });
        }
        else
        {
            // The host owns the session. The parameterless overload only registers the
            // authentication services and hands back a builder; unlike the one above it does not
            // touch AuthenticationOptions, so the host's default schemes survive.
            authBuilder = services.AddAuthentication();
        }

        // Add OpenID Connect handler for BirSign
        authBuilder.AddOpenIdConnect(BirSignConstants.AuthenticationType, options =>
        {
            options.Authority = authority;
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.CallbackPath = new PathString(redirectUri);
            options.RemoteSignOutPath = new PathString(logoutRedirectUri);
            options.SignedOutCallbackPath = new PathString(postLogoutRedirectUri);

            if (signInScheme is not null)
            {
                options.SignInScheme = signInScheme;
            }

            // For enforced security
#if NET9_0_OR_GREATER
            options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.UseIfAvailable;
#endif

            // Require PKCE for added security
            options.UsePkce = true;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.ResponseMode = OpenIdConnectResponseMode.Query;

            // Fetch user info from the UserInfo endpoint
            options.GetClaimsFromUserInfoEndpoint = true;

            // Scopes: Request openid and profile by default; add more as needed
            options.Scope.Add(OpenIdConnectScope.OpenId);
            options.Scope.Add(OpenIdConnectScope.Profile);

            // Token validation parameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                NameClaimType = "name",
                RoleClaimType = "role",
            };

            options.Events = new OpenIdConnectEvents
            {
#if !NET9_0_OR_GREATER
                // Before .NET 9 there is no built-in PAR support, so push the request by hand.
                OnRedirectToIdentityProvider = ParHelper.EnablePar,
#endif
                OnTokenValidated = async (context) =>
                {
                    var identity = context.Principal!.Identity as ClaimsIdentity;
                    identity!.AddUserRoles();

                    var sub = identity?.FindFirst("sub")?.Value 
                              ?? identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(sub))
                    {
                        var memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
                        memoryCache?.Remove($"RevokedUser_{sub}");
                    }

                    if (manageUser is not null)
                    {
                        var serviceProvider = context.HttpContext.RequestServices;
                        await manageUser(serviceProvider, identity!);
                    }
                },
            };

            // Allow further customization via the provided action
            optionsConfigurator?.Invoke(options);

            // Already set from configuration above; repeat it only in case the configurator
            // repointed the authority.
            if (!string.Equals(options.Authority, BirSignSettings.Authority, StringComparison.Ordinal))
            {
                BirSignSettings.SetAuthority(options.Authority);
            }
        });

        if (webhookHandler != null && !string.IsNullOrEmpty(webhookUrl))
        {
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new WebhookStartupFilter(webhookUrl, webhookHandler));
        }

        return services;
    }
}
