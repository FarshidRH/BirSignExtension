using Microsoft.AspNetCore.Identity;

namespace MvcNetCore.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }

        public string? Family { get; set; }

        public string? BirthDay { get; set; }

        public string? NationalCode { get; set; }
    }
}
