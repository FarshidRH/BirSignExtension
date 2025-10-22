using System;
using System.Configuration;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Constants
{
    public class BirSignConstants
    {
        public const string AuthenticationType = "ApplicationCookie";
        public const string ResponseType = "id_token";
        public const string Scope = "openid profile";
        public const string LoginUri = "/BirSign/Login";

        public static bool IsUseBirSign =
            ConfigurationManager.AppSettings["IsUseBirSign"] != null &&
            Convert.ToBoolean(ConfigurationManager.AppSettings["IsUseBirSign"]);
    }
}
