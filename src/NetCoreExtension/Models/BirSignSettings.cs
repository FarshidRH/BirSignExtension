using Microsoft.Extensions.Configuration;

namespace MapIdeaHub.BirSign.NetCoreExtension.Models
{
    public class BirSignSettings
    {
        public static bool IsUseBirSign(IConfiguration configuration)
            => configuration?.GetValue<bool>("BirSign:IsUse") ?? false;

        internal static string? Authority { get; set; }
        public static string? RegisterUri { get; internal set; }
    }
}
