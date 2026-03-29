using Microsoft.EntityFrameworkCore;
using PurchaseManagement.API.Data;
using PurchaseManagement.API.Entities;

namespace PurchaseManagement.API.Repositories
{
    /// <summary>
    /// Concrete implementation of IItemRepository using EF Core.
    /// Provides Item master data access (Task 2 - GET /api/items).
    /// </summary>
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all active items ordered by name</summary>
        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await _context.Items
                .Where(i => i.IsActive)
                .OrderBy(i => i.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>Returns a single item by its primary key</summary>
        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <summary>Returns items whose names contain the search query (Autocomplete support)</summary>
        public async Task<IEnumerable<Item>> SearchAsync(string query)
        {
            return await _context.Items
                .Where(i => i.IsActive && i.Name.Contains(query))
                .OrderBy(i => i.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }

    /// <summary>
    /// Concrete implementation of ILocationRepository using EF Core.
    /// Provides Location/Batch master data access (Task 2 - GET /api/locations).
    /// </summary>
    public class LocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all active locations ordered by code</summary>
        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await _context.Locations
                .Where(l => l.IsActive)
                .OrderBy(l => l.Code)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>Returns a single location by its primary key</summary>
        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }

    /// <summary>
    /// Concrete implementation of IPurchaseBillRepository using EF Core.
    /// Handles Create, Update, and Read operations for Purchase Bills (Tasks 3, 4, 5.2).
    /// </summary>
    public class PurchaseBillRepository : IPurchaseBillRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseBillRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all purchase bills as lightweight summaries</summary>
        public async Task<IEnumerable<PurchaseBill>> GetAllAsync()
        {
            return await _context.PurchaseBills
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns a single purchase bill with all its line items and related entities.
        /// Used for Edit (Task 5.2) and PDF generation (Task 5.1).
        /// </summary>
        public async Task<PurchaseBill?> GetByIdAsync(int id)
        {
            return await _context.PurchaseBills
                .Include(b => b.Items)
                    .ThenInclude(i => i.Item)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Location)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        /// <summary>Persists a new purchase bill and its line items to the database</summary>
        public async Task<PurchaseBill> CreateAsync(PurchaseBill bill)
        {
            _context.PurchaseBills.Add(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

        /// <summary>
        /// Updates an existing purchase bill.
        /// The service layer is responsible for removing old items and adding new ones.
        /// </summary>
        public async Task<PurchaseBill> UpdateAsync(PurchaseBill bill)
        {
            bill.UpdatedAt = DateTime.UtcNow;
            _context.PurchaseBills.Update(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

        /// <summary>
        /// Generates a unique sequential bill number in the format PB-YYYYNNNN.
        /// Example: PB-20240001
        /// </summary>
        public async Task<string> GenerateBillNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"PB-{year}";

            // Count bills created in the current year to determine next sequence
            var countThisYear = await _context.PurchaseBills
                .CountAsync(b => b.BillNumber.StartsWith(prefix));

            var sequence = (countThisYear + 1).ToString("D4");
            return $"{prefix}{sequence}";
        }
    }

    /// <summary>
    /// Concrete implementation of IAuditLogRepository using EF Core.
    /// Stores audit trail records for Create/Update actions (Task 5.4).
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Adds a new audit log entry to the database</summary>
        public async Task AddAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>Returns all audit logs for a given entity (and optionally a specific record)</summary>
        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity, int? entityId = null)
        {
            var query = _context.AuditLogs
                .Where(a => a.Entity == entity);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>Returns the most recent audit log entries across all entities</summary>
        public async Task<IEnumerable<AuditLog>> GetAllAsync(int pageSize = 50)
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
