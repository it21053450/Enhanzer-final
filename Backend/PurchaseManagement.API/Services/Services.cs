using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.API.Data;
using PurchaseManagement.API.DTOs;
using PurchaseManagement.API.Entities;
using PurchaseManagement.API.Repositories;

namespace PurchaseManagement.API.Services
{
    /// <summary>
    /// Business logic service for Item master data.
    /// Handles mapping from entities to DTOs.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        /// <summary>Returns all active items as DTOs</summary>
        public async Task<IEnumerable<ItemDto>> GetAllItemsAsync()
        {
            var items = await _itemRepository.GetAllAsync();
            return items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                IsActive = i.IsActive
            });
        }

        /// <summary>Returns items matching the search query for autocomplete</summary>
        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string query)
        {
            var items = await _itemRepository.SearchAsync(query);
            return items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                IsActive = i.IsActive
            });
        }
    }

    /// <summary>
    /// Business logic service for Location master data.
    /// Locations are displayed as the "Batch" dropdown in the Purchase Bill form.
    /// </summary>
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        /// <summary>Returns all active locations as DTOs</summary>
        public async Task<IEnumerable<LocationDto>> GetAllLocationsAsync()
        {
            var locations = await _locationRepository.GetAllAsync();
            return locations.Select(l => new LocationDto
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                IsActive = l.IsActive
            });
        }
    }

    /// <summary>
    /// Core business logic for Purchase Bill operations.
    /// Handles:
    ///   - Real-time calculations: TotalCost, TotalSelling, summary totals
    ///   - Bill number generation
    ///   - Audit trail logging on Create and Update (Task 5.4)
    ///   - Entity-to-DTO mapping
    /// </summary>
    public class PurchaseBillService : IPurchaseBillService
    {
        private readonly IPurchaseBillRepository _billRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ApplicationDbContext _context;

        public PurchaseBillService(
            IPurchaseBillRepository billRepository,
            IAuditLogRepository auditLogRepository,
            ApplicationDbContext context)
        {
            _billRepository = billRepository;
            _auditLogRepository = auditLogRepository;
            _context = context;
        }

        /// <summary>Returns a summary list of all purchase bills</summary>
        public async Task<IEnumerable<PurchaseBillSummaryDto>> GetAllBillsAsync()
        {
            var bills = await _billRepository.GetAllAsync();
            return bills.Select(b => new PurchaseBillSummaryDto
            {
                Id = b.Id,
                BillNumber = b.BillNumber,
                BillDate = b.BillDate,
                Status = b.Status,
                TotalItems = b.TotalItems,
                TotalQuantity = b.TotalQuantity,
                TotalAmount = b.TotalAmount,
                CreatedAt = b.CreatedAt
            });
        }

        /// <summary>Returns a full purchase bill with all line items by ID</summary>
        public async Task<PurchaseBillDto?> GetBillByIdAsync(int id)
        {
            var bill = await _billRepository.GetByIdAsync(id);
            if (bill == null) return null;
            return MapToDto(bill);
        }

        /// <summary>
        /// Creates a new Purchase Bill with all its line items.
        /// Performs:
        ///   1. Bill number generation
        ///   2. Per-line calculation: TotalCost and TotalSelling
        ///   3. Summary calculation: TotalItems, TotalQuantity, TotalAmount
        ///   4. DB persistence
        ///   5. Audit log entry (Action = "Create")
        /// </summary>
        public async Task<ApiResponse<PurchaseBillDto>> CreateBillAsync(CreatePurchaseBillDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return ApiResponse<PurchaseBillDto>.Fail("At least one item is required.");

            // Generate unique bill number
            var billNumber = await _billRepository.GenerateBillNumberAsync();

            // Build the bill entity with calculated values
            var bill = new PurchaseBill
            {
                BillNumber = billNumber,
                BillDate = dto.BillDate,
                Notes = dto.Notes,
                Status = "Saved",
                CreatedAt = DateTime.UtcNow
            };

            // Map and calculate each line item
            var sortOrder = 1;
            foreach (var itemDto in dto.Items)
            {
                var lineItem = CalculateLineItem(itemDto, sortOrder++);
                bill.Items.Add(lineItem);
            }

            // Compute bill-level summary totals
            UpdateBillSummary(bill);

            // Persist to database
            await _billRepository.CreateAsync(bill);

            // Write audit log: Create action
            await _auditLogRepository.AddAsync(new AuditLog
            {
                Entity = "PurchaseBill",
                Action = "Create",
                EntityId = bill.Id,
                OldValue = null,  // No previous value on create
                NewValue = JsonSerializer.Serialize(new { bill.BillNumber, bill.BillDate, bill.TotalAmount }),
                CreatedAt = DateTime.UtcNow
            });

            // Reload to include navigation properties for the response
            var created = await _billRepository.GetByIdAsync(bill.Id);
            return ApiResponse<PurchaseBillDto>.Ok(MapToDto(created!), "Purchase Bill created successfully.");
        }

        /// <summary>
        /// Updates an existing Purchase Bill (Task 5.2 - Edit Purchase Bill).
        /// Replaces all line items with the new collection, recalculates totals,
        /// and writes an audit log entry with old vs. new values.
        /// </summary>
        public async Task<ApiResponse<PurchaseBillDto>> UpdateBillAsync(int id, UpdatePurchaseBillDto dto)
        {
            var existing = await _context.PurchaseBills
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existing == null)
                return ApiResponse<PurchaseBillDto>.Fail($"Purchase Bill with ID {id} not found.");

            if (dto.Items == null || dto.Items.Count == 0)
                return ApiResponse<PurchaseBillDto>.Fail("At least one item is required.");

            // Capture old state for audit log
            var oldSnapshot = JsonSerializer.Serialize(new
            {
                existing.BillNumber,
                existing.BillDate,
                existing.TotalAmount,
                existing.TotalItems,
                existing.TotalQuantity
            });

            // Remove all existing line items (cascade handled by EF)
            _context.PurchaseBillItems.RemoveRange(existing.Items);

            // Update header fields
            existing.BillDate = dto.BillDate;
            existing.Notes = dto.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Items.Clear();

            // Re-add line items with recalculated values
            var sortOrder = 1;
            foreach (var itemDto in dto.Items)
            {
                var lineItem = CalculateLineItem(itemDto, sortOrder++);
                existing.Items.Add(lineItem);
            }

            // Recalculate summary totals
            UpdateBillSummary(existing);

            await _context.SaveChangesAsync();

            // Write audit log: Update action with before/after snapshot
            var newSnapshot = JsonSerializer.Serialize(new
            {
                existing.BillNumber,
                existing.BillDate,
                existing.TotalAmount,
                existing.TotalItems,
                existing.TotalQuantity
            });

            await _auditLogRepository.AddAsync(new AuditLog
            {
                Entity = "PurchaseBill",
                Action = "Update",
                EntityId = existing.Id,
                OldValue = oldSnapshot,
                NewValue = newSnapshot,
                CreatedAt = DateTime.UtcNow
            });

            var updated = await _billRepository.GetByIdAsync(existing.Id);
            return ApiResponse<PurchaseBillDto>.Ok(MapToDto(updated!), "Purchase Bill updated successfully.");
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Calculates TotalCost and TotalSelling for a single line item.
        /// TotalCost    = (Cost × Qty) - (Cost × Qty × DiscountPercent / 100)
        /// TotalSelling = Price × Qty
        /// </summary>
        private static PurchaseBillItem CalculateLineItem(CreatePurchaseBillItemDto dto, int sortOrder)
        {
            var grossCost = dto.Cost * dto.Quantity;
            var discountAmount = grossCost * (dto.DiscountPercent / 100m);
            var totalCost = grossCost - discountAmount;
            var totalSelling = dto.Price * dto.Quantity;

            return new PurchaseBillItem
            {
                ItemId = dto.ItemId,
                LocationId = dto.LocationId,
                Cost = dto.Cost,
                Price = dto.Price,
                Quantity = dto.Quantity,
                DiscountPercent = dto.DiscountPercent,
                TotalCost = Math.Round(totalCost, 2),
                TotalSelling = Math.Round(totalSelling, 2),
                SortOrder = sortOrder
            };
        }

        /// <summary>
        /// Updates the bill-level summary totals from its line items.
        /// TotalItems    = Count of distinct rows
        /// TotalQuantity = SUM of Quantity across all rows
        /// TotalAmount   = SUM of TotalSelling across all rows
        /// </summary>
        private static void UpdateBillSummary(PurchaseBill bill)
        {
            bill.TotalItems = bill.Items.Count;
            bill.TotalQuantity = bill.Items.Sum(i => i.Quantity);
            bill.TotalAmount = bill.Items.Sum(i => i.TotalSelling);
        }

        /// <summary>Maps a PurchaseBill entity (with navigation properties) to a full DTO</summary>
        private static PurchaseBillDto MapToDto(PurchaseBill bill)
        {
            return new PurchaseBillDto
            {
                Id = bill.Id,
                BillNumber = bill.BillNumber,
                BillDate = bill.BillDate,
                Notes = bill.Notes,
                Status = bill.Status,
                TotalItems = bill.TotalItems,
                TotalQuantity = bill.TotalQuantity,
                TotalAmount = bill.TotalAmount,
                CreatedAt = bill.CreatedAt,
                UpdatedAt = bill.UpdatedAt,
                Items = bill.Items
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new PurchaseBillItemDto
                    {
                        Id = i.Id,
                        ItemId = i.ItemId,
                        ItemName = i.Item?.Name ?? string.Empty,
                        LocationId = i.LocationId,
                        LocationCode = i.Location?.Code ?? string.Empty,
                        LocationName = i.Location?.Name ?? string.Empty,
                        Cost = i.Cost,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        DiscountPercent = i.DiscountPercent,
                        TotalCost = i.TotalCost,
                        TotalSelling = i.TotalSelling,
                        SortOrder = i.SortOrder
                    }).ToList()
            };
        }
    }

    /// <summary>
    /// Service for retrieving audit log entries (Task 5.4).
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(int pageSize = 50)
        {
            var logs = await _auditLogRepository.GetAllAsync(pageSize);
            return logs.Select(MapToDto);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsByEntityAsync(string entity, int? entityId = null)
        {
            var logs = await _auditLogRepository.GetByEntityAsync(entity, entityId);
            return logs.Select(MapToDto);
        }

        private static AuditLogDto MapToDto(AuditLog log) => new()
        {
            Id = log.Id,
            Entity = log.Entity,
            Action = log.Action,
            OldValue = log.OldValue,
            NewValue = log.NewValue,
            EntityId = log.EntityId,
            CreatedAt = log.CreatedAt
        };
    }
}
