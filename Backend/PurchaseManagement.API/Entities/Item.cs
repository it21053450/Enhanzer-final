namespace PurchaseManagement.API.Entities
{
    /// <summary>
    /// Represents a product/item available for purchase.
    /// Seeded with predefined fruits as per assignment requirements.
    /// </summary>
    public class Item
    {
        public int Id { get; set; }

        /// <summary>Name of the item (e.g., Mango, Apple)</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Indicates if the item is active/visible</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Timestamp when item was created</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property - items used in purchase bill lines
        public ICollection<PurchaseBillItem> PurchaseBillItems { get; set; } = new List<PurchaseBillItem>();
    }
}
