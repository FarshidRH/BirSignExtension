using System;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    public class UserPositionApiDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public Guid PositionId { get; set; }
        public string PositionTitle { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentTitle { get; set; }
        public int PositionType { get; set; } // 1=Chief, 2=Expert
        public int Row { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
