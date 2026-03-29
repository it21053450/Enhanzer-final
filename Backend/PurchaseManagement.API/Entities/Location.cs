namespace PurchaseManagement.API.Entities
{
    /// <summary>
    /// Represents a storage location (warehouse/store).
    /// Used as "Batch" dropdown in the Purchase Bill form.
    /// Seeded with LOC001, LOC002, LOC003 per assignment requirements.
    /// </summary>
    public class Location
    {
        public int Id { get; set; }

        /// <summary>Unique location code (e.g., LOC001)</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Human-readable location name (e.g., Warehouse A)</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Indicates if this location is active</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Timestamp when location was created</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property - lines that use this location as batch
        public ICollection<PurchaseBillItem> PurchaseBillItems { get; set; } = new List<PurchaseBillItem>();
    }
}
