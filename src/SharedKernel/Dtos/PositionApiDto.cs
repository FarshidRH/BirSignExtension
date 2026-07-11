using MapIdeaHub.BirSign.SharedKernel.Enums;
using System;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    public class PositionApiDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentTitle { get; set; }
        public string Description { get; set; }
        public string QualificationCriteria { get; set; }
        public PositionType PositionType { get; set; }
        public int Row { get; set; }
    }
}
