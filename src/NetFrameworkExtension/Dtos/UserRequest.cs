namespace MapIdeaHub.BirSign.NetFrameworkExtension.Dtos
{
    public class UserRequest
    {
        public string UserName { get; set; }

        public string NationalCode { get; set; }

        public string BirthDate { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; } = false;

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ActiveDirectoryUser { get; set; }

        //public GenderType? Gender { get; set; } = GenderType.UnSpesified;
    }
}
