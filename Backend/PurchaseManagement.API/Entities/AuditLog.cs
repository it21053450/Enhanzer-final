namespace PurchaseManagement.API.Entities
{
    /// <summary>
    /// Audit trail record for tracking Create/Update operations on Purchase Bills.
    /// Required by Task 5.4 - Audit Trail (MANDATORY).
    /// 
    /// Fields per assignment:
    ///   Id, Entity, Action, OldValue, NewValue, CreatedAt
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        /// <summary>Name of the entity being audited (e.g., "PurchaseBill")</summary>
        public string Entity { get; set; } = string.Empty;

        /// <summary>Action performed: "Create" or "Update"</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>JSON snapshot of the entity BEFORE the change (null for Create)</summary>
        public string? OldValue { get; set; }

        /// <summary>JSON snapshot of the entity AFTER the change</summary>
        public string? NewValue { get; set; }

        /// <summary>ID of the affected entity record</summary>
        public int? EntityId { get; set; }

        /// <summary>Timestamp when this audit entry was created</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
