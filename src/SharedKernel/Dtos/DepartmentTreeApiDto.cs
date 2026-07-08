using System.Collections.Generic;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    public class DepartmentTreeApiDto
    {
        public DepartmentApiDto Department { get; set; } = new DepartmentApiDto();
        public List<PositionApiDto> Positions { get; set; } = new List<PositionApiDto>();
        public List<UserPositionApiDto> UserPositions { get; set; } = new List<UserPositionApiDto>();
        public List<UserPositionApiDto> ActiveAssignments { get; set; } = new List<UserPositionApiDto>();
    }
}
