using System.Collections.Generic;
using System.Security.Claims;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Dtos
{
    public class UserInfo
    {
        public string Name { get; set; }

        public string Family { get; set; }

        public string NationalCode { get; set; }

        public string BirthDate { get; set; }

        public string Gender { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public ClaimsIdentity Identity { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }
}
