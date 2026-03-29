using PurchaseManagement.API.DTOs;

namespace PurchaseManagement.API.Services
{
    /// <summary>
    /// Service interface for Item master data business operations.
    /// </summary>
    public interface IItemService
    {
        Task<IEnumerable<ItemDto>> GetAllItemsAsync();
        Task<IEnumerable<ItemDto>> SearchItemsAsync(string query);
    }

    /// <summary>
    /// Service interface for Location master data business operations.
    /// </summary>
    public interface ILocationService
    {
        Task<IEnumerable<LocationDto>> GetAllLocationsAsync();
    }

    /// <summary>
    /// Service interface for Purchase Bill business operations.
    /// Covers Tasks 3, 4, and 5.2 (Create, Read, Update).
    /// </summary>
    public interface IPurchaseBillService
    {
        Task<IEnumerable<PurchaseBillSummaryDto>> GetAllBillsAsync();
        Task<PurchaseBillDto?> GetBillByIdAsync(int id);
        Task<ApiResponse<PurchaseBillDto>> CreateBillAsync(CreatePurchaseBillDto dto);
        Task<ApiResponse<PurchaseBillDto>> UpdateBillAsync(int id, UpdatePurchaseBillDto dto);
    }

    /// <summary>
    /// Service interface for Audit Log retrieval.
    /// </summary>
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(int pageSize = 50);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsByEntityAsync(string entity, int? entityId = null);
    }
}
