namespace PurchaseManagement.API.Entities
{
    /// <summary>
    /// Represents the header of a Purchase Bill transaction.
    /// A purchase bill contains one header and multiple line items.
    /// </summary>
    public class PurchaseBill
    {
        public int Id { get; set; }

        /// <summary>Auto-generated bill number (e.g., PB-20240001)</summary>
        public string BillNumber { get; set; } = string.Empty;

        /// <summary>Date the purchase bill was created</summary>
        public DateTime BillDate { get; set; } = DateTime.UtcNow;

        /// <summary>Optional supplier reference or notes</summary>
        public string? Notes { get; set; }

        /// <summary>Total count of distinct items in this bill</summary>
        public int TotalItems { get; set; }

        /// <summary>Sum of all quantities across all line items</summary>
        public decimal TotalQuantity { get; set; }

        /// <summary>Sum of Total Selling (Price × Qty) across all lines</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Current status of the bill (Draft, Saved)</summary>
        public string Status { get; set; } = "Draft";

        /// <summary>Timestamp when the bill was created</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Timestamp when the bill was last updated</summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation property - line items belonging to this bill
        public ICollection<PurchaseBillItem> Items { get; set; } = new List<PurchaseBillItem>();
    }
}
