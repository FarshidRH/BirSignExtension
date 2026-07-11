using System;
using System.Collections.Generic;

namespace MvcNetCore.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid? ParentDepartmentId { get; set; }
        public string? FullName { get; set; }
        public int Level { get; set; }

        // Navigation properties
        public virtual Department? ParentDepartment { get; set; }
        public virtual ICollection<Department> Children { get; set; } = new List<Department>();
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
