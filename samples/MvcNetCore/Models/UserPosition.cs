using System;

namespace MvcNetCore.Models
{
    public class UserPosition
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid PositionId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Position? Position { get; set; }
    }
}
