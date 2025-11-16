using System;
using System.Configuration;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Constants
{
    public class BirSignConstants
    {
        public static bool IsUseBirSign =
            ConfigurationManager.AppSettings["IsUseBirSign"] != null &&
            Convert.ToBoolean(ConfigurationManager.AppSettings["IsUseBirSign"]);

        public const string AuthenticationType = "BirSign";
        public const string LoginUri = "/BirSign/Login";

        internal static string Authority { get; set; }
        public static string RegisterUri { get; internal set; }
    }
}
