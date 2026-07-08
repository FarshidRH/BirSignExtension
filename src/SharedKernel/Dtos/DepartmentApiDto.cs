using System;
using System.Collections.Generic;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    public class DepartmentApiDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid? ParentDepartmentId { get; set; }
        public string FullName { get; set; }
        public int Level { get; set; }
        public List<DepartmentApiDto> Children { get; set; } = new List<DepartmentApiDto>();
    }
}
