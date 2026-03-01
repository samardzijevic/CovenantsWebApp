using System;

namespace Covenants.Models
{
    public class CovenantHistory
    {
        public int Id { get; set; }
        public int CovenantId { get; set; }
        public string Action { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string Notes { get; set; }
    }
}
