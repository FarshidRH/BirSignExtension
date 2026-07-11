using System;
using System.Collections.Generic;
using MapIdeaHub.BirSign.SharedKernel.Enums;

namespace MvcNetCore.Models
{
    public class Position
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string? Description { get; set; }
        public string? QualificationCriteria { get; set; }
        public PositionType PositionType { get; set; }
        public int Row { get; set; }

        // Navigation properties
        public virtual Department? Department { get; set; }
        public virtual ICollection<UserPosition> UserPositions { get; set; } = new List<UserPosition>();
    }
}
