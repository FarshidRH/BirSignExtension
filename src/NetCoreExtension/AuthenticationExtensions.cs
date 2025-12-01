using MapIdeaHub.BirSign.NetCoreExtension.Models;
using MapIdeaHub.BirSign.SharedKernel.Constants;
using MapIdeaHub.BirSign.SharedKernel.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection BirSignAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, ClaimsIdentity, Task>? manageUser = null,
        Action<OpenIdConnectOptions>? optionsConfigurator = null)
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

        // Add authentication services
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = BirSignConstants.AuthenticationType;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        // Add cookie authentication for session management
        authBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.LogoutPath = "/Identity/Account/Logout";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        // Add OpenID Connect handler for BirSign
        authBuilder.AddOpenIdConnect(BirSignConstants.AuthenticationType, options =>
        {
            options.Authority = authority;
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.CallbackPath = new PathString(redirectUri);
            options.RemoteSignOutPath = new PathString(logoutRedirectUri);
            options.SignedOutCallbackPath = new PathString(postLogoutRedirectUri);

            // For enforced security
            options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.UseIfAvailable;

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
                OnTokenValidated = async (context) =>
                {
                    var identity = context.Principal!.Identity as ClaimsIdentity;
                    identity!.AddUserRoles();

                    if (manageUser is not null)
                    {
                        var serviceProvider = context.HttpContext.RequestServices;
                        await manageUser(serviceProvider, identity!);
                    }
                },
            };

            // Allow further customization via the provided action
            optionsConfigurator?.Invoke(options);

            BirSignSettings.Authority = options.Authority;
            BirSignSettings.RegisterUri = $"{options.Authority?.TrimEnd('/')}/Account/Register";
        });

        return services;
    }
}
