using System;

namespace Covenants.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int CovenantId { get; set; }
        public string CovenantTitle { get; set; }  // joined for display
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DismissedAt { get; set; }
    }
}
