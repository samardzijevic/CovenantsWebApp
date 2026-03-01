using System;

namespace Covenants.Models
{
    public class Covenant
    {
        public int Id { get; set; }
        public int CovenantTypeId { get; set; }
        public string CovenantTypeName { get; set; }   // joined for display
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ProcessingDate { get; set; }
        public decimal? Value { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
