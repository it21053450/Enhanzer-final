using PurchaseManagement.API.Entities;

namespace PurchaseManagement.API.Repositories
{
    /// <summary>
    /// Repository interface for Item master data operations.
    /// Part of the Repository pattern for clean architecture separation.
    /// </summary>
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<IEnumerable<Item>> SearchAsync(string query);
    }

    /// <summary>
    /// Repository interface for Location master data operations.
    /// Locations serve as the "Batch" dropdown in the Purchase Bill form.
    /// </summary>
    public interface ILocationRepository
    {
        Task<IEnumerable<Location>> GetAllAsync();
        Task<Location?> GetByIdAsync(int id);
    }

    /// <summary>
    /// Repository interface for Purchase Bill CRUD operations.
    /// </summary>
    public interface IPurchaseBillRepository
    {
        Task<IEnumerable<Entities.PurchaseBill>> GetAllAsync();
        Task<Entities.PurchaseBill?> GetByIdAsync(int id);
        Task<Entities.PurchaseBill> CreateAsync(Entities.PurchaseBill bill);
        Task<Entities.PurchaseBill> UpdateAsync(Entities.PurchaseBill bill);
        Task<string> GenerateBillNumberAsync();
    }

    /// <summary>
    /// Repository interface for Audit Log operations (Task 5.4).
    /// </summary>
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity, int? entityId = null);
        Task<IEnumerable<AuditLog>> GetAllAsync(int pageSize = 50);
    }
}
