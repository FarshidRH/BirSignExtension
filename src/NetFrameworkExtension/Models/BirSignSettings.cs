using System;
using System.Configuration;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Models
{
    public class BirSignSettings
    {
        public static bool IsUseBirSign =
            ConfigurationManager.AppSettings["BirSign:IsUse"] != null &&
            Convert.ToBoolean(ConfigurationManager.AppSettings["BirSign:IsUse"]);

        internal static string Authority { get; set; }
        public static string RegisterUri { get; internal set; }
    }
}
