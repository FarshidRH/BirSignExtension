using System.Collections.Generic;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    public class ClaimInfo
    {
        public string ClaimType { get; set; }
        public string Description { get; set; }
        public ClaimValueType ValueType { get; set; }
        public List<string> PredefinedValues { get; set; }
    }

    public enum ClaimValueType
    {
        Boolean = 0,
        FreeText = 1,
        PredefinedList = 2
    }
}
