using Microsoft.Extensions.Configuration;

namespace MapIdeaHub.BirSign.NetCoreExtension.Models;

public class BirSignSettings
{
    public static bool IsUseBirSign(IConfiguration configuration)
        => configuration?.GetValue<bool>("BirSign:IsUse") ?? false;

    internal static string? Authority { get; set; }

    /// <summary>BirSign's sign-up page, for a "create an account" link.</summary>
    public static string? RegisterUri { get; private set; }

    /// <summary>BirSign's password-recovery page, for a "forgot your password" link.</summary>
    /// <remarks>
    /// Worth offering wherever an account can be created from outside BirSign: the registration
    /// API creates users without a password, so recovery is how they get their first one.
    /// </remarks>
    public static string? ForgotPasswordUri { get; private set; }

    /// <summary>BirSign's self-service account page, where a user edits their own profile.</summary>
    public static string? ManageUri { get; private set; }

    /// <summary>
    /// Points the public URIs at an authority. Called during
    /// <c>AddBirSignAuthentication</c> so they are set before the first request, not on the
    /// first challenge — a login page needs them earlier than that.
    /// </summary>
    internal static void SetAuthority(string? authority)
    {
        Authority = authority;

        var baseUri = authority?.TrimEnd('/');
        RegisterUri = baseUri is null ? null : $"{baseUri}/Account/Register";
        ForgotPasswordUri = baseUri is null ? null : $"{baseUri}/Account/ForgotPassword";
        ManageUri = baseUri is null ? null : $"{baseUri}/Manage";
    }
}
