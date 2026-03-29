using Microsoft.EntityFrameworkCore;
using PurchaseManagement.API.Entities;

namespace PurchaseManagement.API.Data
{
    /// <summary>
    /// Entity Framework Core database context for the Purchase Management system.
    /// Configures all entity mappings and seeds initial master data
    /// (Locations and Items) as required by Task 1.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── DbSets ────────────────────────────────────────────────────────────────
        public DbSet<Item> Items { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<PurchaseBill> PurchaseBills { get; set; }
        public DbSet<PurchaseBillItem> PurchaseBillItems { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Item configuration ────────────────────────────────────────────────
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ── Location configuration ────────────────────────────────────────────
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // ── PurchaseBill configuration ─────────────────────────────────────────
            modelBuilder.Entity<PurchaseBill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BillNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.BillNumber).IsUnique();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalQuantity).HasColumnType("decimal(18,2)");
            });

            // ── PurchaseBillItem configuration ────────────────────────────────────
            modelBuilder.Entity<PurchaseBillItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalSelling).HasColumnType("decimal(18,2)");

                // FK: PurchaseBillItem → PurchaseBill (cascade delete)
                entity.HasOne(e => e.PurchaseBill)
                      .WithMany(b => b.Items)
                      .HasForeignKey(e => e.PurchaseBillId)
                      .OnDelete(DeleteBehavior.Cascade);

                // FK: PurchaseBillItem → Item (restrict delete)
                entity.HasOne(e => e.Item)
                      .WithMany(i => i.PurchaseBillItems)
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                // FK: PurchaseBillItem → Location (restrict delete)
                entity.HasOne(e => e.Location)
                      .WithMany(l => l.PurchaseBillItems)
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── AuditLog configuration ─────────────────────────────────────────────
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Entity).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                // OldValue and NewValue store JSON - use nvarchar(max)
                entity.Property(e => e.OldValue).HasColumnType("nvarchar(max)");
                entity.Property(e => e.NewValue).HasColumnType("nvarchar(max)");
            });

            // ── Seed Data (Task 1) ────────────────────────────────────────────────
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Seeds the initial master data required before the application runs.
        /// Locations: LOC001 – Warehouse A, LOC002 – Warehouse B, LOC003 – Main Store
        /// Items: Mango, Apple, Banana, Orange, Grapes, Kiwi, Strawberry
        /// </summary>
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Locations
            modelBuilder.Entity<Location>().HasData(
                new Location { Id = 1, Code = "LOC001", Name = "Warehouse A", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Location { Id = 2, Code = "LOC002", Name = "Warehouse B", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Location { Id = 3, Code = "LOC003", Name = "Main Store",  IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // Seed Items (fruits as specified)
            modelBuilder.Entity<Item>().HasData(
                new Item { Id = 1, Name = "Mango",      IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Item { Id = 2, Name = "Apple",      IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Item { Id = 3, Name = "Banana",     IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Item { Id = 4, Name = "Orange",     IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Item { Id = 5, Name = "Grapes",     IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Item { Id = 6, Name = "Kiwi",       IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Item { Id = 7, Name = "Strawberry", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
