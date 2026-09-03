using System;
using System.Configuration;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Models
{
    public class BirSignSettings
    {
        public static bool IsUseBirSign =
            ConfigurationManager.AppSettings["BirSign:IsUse"] != null &&
            Convert.ToBoolean(ConfigurationManager.AppSettings["BirSign:IsUse"]);

        /// <summary>BirSign's sign-up page, for a "create an account" link.</summary>
        public static string RegisterUri { get; private set; }

        /// <summary>BirSign's password-recovery page, for a "forgot your password" link.</summary>
        /// <remarks>
        /// Worth offering wherever an account can be created from outside BirSign: the registration
        /// API creates users without a password, so recovery is how they get their first one.
        /// </remarks>
        public static string ForgotPasswordUri { get; private set; }

        /// <summary>BirSign's self-service account page, where a user edits their own profile.</summary>
        public static string ManageUri { get; private set; }

        internal static string Authority { get; private set; }
        internal static string PushAuthorizationRequestEndpoint { get; set; }

        /// <summary>
        /// Points the public URIs at an authority. Called from <c>UseBirSignAuthentication</c>.
        /// </summary>
        internal static void SetAuthority(string authority)
        {
            Authority = authority;

            var baseUri = authority?.TrimEnd('/');
            RegisterUri = baseUri == null ? null : $"{baseUri}/Account/Register";
            ForgotPasswordUri = baseUri == null ? null : $"{baseUri}/Account/ForgotPassword";
            ManageUri = baseUri == null ? null : $"{baseUri}/Manage";
        }
    }
}
