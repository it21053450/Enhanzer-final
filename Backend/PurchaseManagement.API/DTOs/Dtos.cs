namespace PurchaseManagement.API.DTOs
{
    /// <summary>
    /// Data Transfer Object for Item master data.
    /// Returned by GET /api/items
    /// </summary>
    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Location master data.
    /// Returned by GET /api/locations
    /// </summary>
    public class LocationDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // ── Purchase Bill DTOs ────────────────────────────────────────────────────────

    /// <summary>
    /// DTO for a single line item when creating or updating a Purchase Bill.
    /// </summary>
    public class CreatePurchaseBillItemDto
    {
        public int ItemId { get; set; }

        /// <summary>Location Id used as the Batch identifier</summary>
        public int LocationId { get; set; }

        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }

        /// <summary>Discount percentage (0–100)</summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>Display order of the row in the UI grid</summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// DTO for creating a new Purchase Bill (POST /api/purchase-bill).
    /// Contains header fields and a collection of line items.
    /// </summary>
    public class CreatePurchaseBillDto
    {
        public DateTime BillDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        /// <summary>List of line items for this bill (minimum 1 required)</summary>
        public List<CreatePurchaseBillItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO for updating an existing Purchase Bill (PUT /api/purchase-bill/{id}).
    /// Allows modifying header fields and replacing all line items.
    /// </summary>
    public class UpdatePurchaseBillDto
    {
        public DateTime BillDate { get; set; }
        public string? Notes { get; set; }

        /// <summary>Replaces all existing line items with this new collection</summary>
        public List<CreatePurchaseBillItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO for a single purchase bill line item returned in API responses.
    /// </summary>
    public class PurchaseBillItemDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalSelling { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Full representation of a Purchase Bill for GET responses.
    /// Includes computed summary fields and all line items.
    /// </summary>
    public class PurchaseBillDto
    {
        public int Id { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;

        // Summary totals calculated server-side
        public int TotalItems { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Line item details</summary>
        public List<PurchaseBillItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Lightweight summary DTO for the purchase bill list view.
    /// </summary>
    public class PurchaseBillSummaryDto
    {
        public int Id { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Audit Log DTO ─────────────────────────────────────────────────────────────

    /// <summary>
    /// DTO for returning audit log entries (Task 5.4).
    /// </summary>
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Entity { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int? EntityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Generic API Response ──────────────────────────────────────────────────────

    /// <summary>
    /// Standard API response wrapper for consistent response structure.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
