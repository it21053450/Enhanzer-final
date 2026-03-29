namespace PurchaseManagement.API.Entities
{
    /// <summary>
    /// Represents a single line item in a Purchase Bill.
    /// Contains item reference, batch/location, pricing and quantity details.
    /// Calculations:
    ///   TotalCost = (Cost × Quantity) - (Cost × Quantity × DiscountPercent / 100)
    ///   TotalSelling = Price × Quantity
    /// </summary>
    public class PurchaseBillItem
    {
        public int Id { get; set; }

        
        public int PurchaseBillId { get; set; }

      
        public int ItemId { get; set; }

        
        public int LocationId { get; set; }

        
        public decimal Cost { get; set; }

       
        public decimal Price { get; set; }

        
        public decimal Quantity { get; set; }

        
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Computed: (Cost × Qty) - (Cost × Qty × DiscountPercent / 100)
        /// Stored for reporting purposes
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Computed: Price × Quantity
        /// Stored for reporting purposes
        /// </summary>
        public decimal TotalSelling { get; set; }

       
        public int SortOrder { get; set; }

        // Navigation properties
        public PurchaseBill PurchaseBill { get; set; } = null!;
        public Item Item { get; set; } = null!;
        public Location Location { get; set; } = null!;
    }
}
