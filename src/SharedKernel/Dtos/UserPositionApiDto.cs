using MapIdeaHub.BirSign.SharedKernel.Enums;
using System;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    public class UserPositionApiDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserPhoneNumber { get; set; }
        public string UserEmail { get; set; }
        public string UserBirthday { get; set; }
        public bool IsOrganUser { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public GenderType Gender { get; set; }
        public Guid PositionId { get; set; }
        public string PositionTitle { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentTitle { get; set; }
        public PositionType PositionType { get; set; }
        public int Row { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
