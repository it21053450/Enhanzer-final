-- ============================================================
-- Mini Purchase Management Module — Complete Database Script
-- Author : Odith (Enhanzer Full Stack Developer Assessment)
-- Database: SQL Server / SQL Server LocalDB
-- Date    : 2026-03-29
--
-- Usage:
--   Option A (LocalDB):  Run via SQL Server Management Studio
--                        or: sqlcmd -S "(localdb)\mssqllocaldb" -i DatabaseScript.sql
--   Option B (EF Core):  dotnet run  (auto-migrates and seeds on startup)
--
-- Tables created:
--   Items, Locations, PurchaseBills, PurchaseBillItems, AuditLogs
-- ============================================================


-- ─────────────────────────────────────────────────────────────
-- 1. CREATE DATABASE
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT name FROM sys.databases WHERE name = N'PurchaseManagementDB'
)
BEGIN
    CREATE DATABASE PurchaseManagementDB;
END;
GO

USE PurchaseManagementDB;
GO


-- ─────────────────────────────────────────────────────────────
-- 2. DROP TABLES (safe cleanup for re-runs, child tables first)
-- ─────────────────────────────────────────────────────────────
IF OBJECT_ID(N'PurchaseBillItems', 'U') IS NOT NULL DROP TABLE PurchaseBillItems;
IF OBJECT_ID(N'PurchaseBills',     'U') IS NOT NULL DROP TABLE PurchaseBills;
IF OBJECT_ID(N'AuditLogs',         'U') IS NOT NULL DROP TABLE AuditLogs;
IF OBJECT_ID(N'Items',             'U') IS NOT NULL DROP TABLE Items;
IF OBJECT_ID(N'Locations',         'U') IS NOT NULL DROP TABLE Locations;
GO


-- ─────────────────────────────────────────────────────────────
-- 3. CREATE TABLES
-- ─────────────────────────────────────────────────────────────

-- 3.1  Items — product master data
CREATE TABLE Items (
    Id        INT            NOT NULL IDENTITY(1,1),
    Name      NVARCHAR(200)  NOT NULL,
    IsActive  BIT            NOT NULL DEFAULT 1,
    CreatedAt DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_Items PRIMARY KEY (Id),
    CONSTRAINT UQ_Items_Name UNIQUE (Name)
);
GO

-- 3.2  Locations — warehouse / store master data
CREATE TABLE Locations (
    Id        INT            NOT NULL IDENTITY(1,1),
    Code      NVARCHAR(20)   NOT NULL,
    Name      NVARCHAR(200)  NOT NULL,
    IsActive  BIT            NOT NULL DEFAULT 1,
    CreatedAt DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_Locations PRIMARY KEY (Id),
    CONSTRAINT UQ_Locations_Code UNIQUE (Code)
);
GO

-- 3.3  PurchaseBills — bill header
CREATE TABLE PurchaseBills (
    Id            INT             NOT NULL IDENTITY(1,1),
    BillNumber    NVARCHAR(50)    NOT NULL,
    BillDate      DATETIME2       NOT NULL,
    Notes         NVARCHAR(MAX)   NULL,
    TotalItems    INT             NOT NULL DEFAULT 0,
    TotalQuantity DECIMAL(18,2)   NOT NULL DEFAULT 0,
    TotalAmount   DECIMAL(18,2)   NOT NULL DEFAULT 0,
    Status        NVARCHAR(50)    NOT NULL DEFAULT 'Saved',
    CreatedAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2       NULL,

    CONSTRAINT PK_PurchaseBills PRIMARY KEY (Id),
    CONSTRAINT UQ_PurchaseBills_BillNumber UNIQUE (BillNumber)
);
GO

-- 3.4  PurchaseBillItems — bill line items
--      TotalCost    = (Cost x Qty) x (1 - DiscountPercent / 100)
--      TotalSelling = Price x Qty
CREATE TABLE PurchaseBillItems (
    Id               INT           NOT NULL IDENTITY(1,1),
    PurchaseBillId   INT           NOT NULL,
    ItemId           INT           NOT NULL,
    LocationId       INT           NOT NULL,
    Cost             DECIMAL(18,2) NOT NULL DEFAULT 0,
    Price            DECIMAL(18,2) NOT NULL DEFAULT 0,
    Quantity         DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountPercent  DECIMAL(5,2)  NOT NULL DEFAULT 0,
    TotalCost        DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalSelling     DECIMAL(18,2) NOT NULL DEFAULT 0,
    SortOrder        INT           NOT NULL DEFAULT 0,

    CONSTRAINT PK_PurchaseBillItems PRIMARY KEY (Id),
    CONSTRAINT FK_PurchaseBillItems_PurchaseBills FOREIGN KEY (PurchaseBillId)
        REFERENCES PurchaseBills (Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseBillItems_Items FOREIGN KEY (ItemId)
        REFERENCES Items (Id) ON DELETE NO ACTION,
    CONSTRAINT FK_PurchaseBillItems_Locations FOREIGN KEY (LocationId)
        REFERENCES Locations (Id) ON DELETE NO ACTION
);
GO

-- 3.5  AuditLogs — tracks Create / Update operations on PurchaseBills (Task 5.4)
CREATE TABLE AuditLogs (
    Id        INT            NOT NULL IDENTITY(1,1),
    Entity    NVARCHAR(100)  NOT NULL,           -- 'PurchaseBill'
    Action    NVARCHAR(50)   NOT NULL,           -- 'Create' | 'Update'
    EntityId  INT            NULL,               -- ID of the affected record
    OldValue  NVARCHAR(MAX)  NULL,               -- JSON snapshot before change
    NewValue  NVARCHAR(MAX)  NULL,               -- JSON snapshot after change
    CreatedAt DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id)
);
GO


-- ─────────────────────────────────────────────────────────────
-- 4. INDEXES
-- ─────────────────────────────────────────────────────────────
CREATE INDEX IX_PurchaseBillItems_PurchaseBillId ON PurchaseBillItems (PurchaseBillId);
CREATE INDEX IX_PurchaseBillItems_ItemId         ON PurchaseBillItems (ItemId);
CREATE INDEX IX_PurchaseBillItems_LocationId     ON PurchaseBillItems (LocationId);
CREATE INDEX IX_AuditLogs_Entity                 ON AuditLogs (Entity);
CREATE INDEX IX_AuditLogs_EntityId               ON AuditLogs (EntityId);
GO


-- ─────────────────────────────────────────────────────────────
-- 5. SEED DATA
-- ─────────────────────────────────────────────────────────────

-- 5.1  Items (7 fruit products as per Task 1)
SET IDENTITY_INSERT Items ON;
INSERT INTO Items (Id, Name, IsActive, CreatedAt) VALUES
    (1, N'Mango',      1, '2024-01-01T00:00:00Z'),
    (2, N'Apple',      1, '2024-01-01T00:00:00Z'),
    (3, N'Banana',     1, '2024-01-01T00:00:00Z'),
    (4, N'Orange',     1, '2024-01-01T00:00:00Z'),
    (5, N'Grapes',     1, '2024-01-01T00:00:00Z'),
    (6, N'Kiwi',       1, '2024-01-01T00:00:00Z'),
    (7, N'Strawberry', 1, '2024-01-01T00:00:00Z');
SET IDENTITY_INSERT Items OFF;
GO

-- 5.2  Locations (3 warehouses / stores as per Task 1)
SET IDENTITY_INSERT Locations ON;
INSERT INTO Locations (Id, Code, Name, IsActive, CreatedAt) VALUES
    (1, N'LOC001', N'Warehouse A', 1, '2024-01-01T00:00:00Z'),
    (2, N'LOC002', N'Warehouse B', 1, '2024-01-01T00:00:00Z'),
    (3, N'LOC003', N'Main Store',  1, '2024-01-01T00:00:00Z');
SET IDENTITY_INSERT Locations OFF;
GO


-- ─────────────────────────────────────────────────────────────
-- 6. VERIFY ROW COUNTS
-- ─────────────────────────────────────────────────────────────
SELECT 'Items'             AS [Table], COUNT(*) AS [Rows] FROM Items
UNION ALL
SELECT 'Locations',                    COUNT(*)            FROM Locations
UNION ALL
SELECT 'PurchaseBills',                COUNT(*)            FROM PurchaseBills
UNION ALL
SELECT 'PurchaseBillItems',            COUNT(*)            FROM PurchaseBillItems
UNION ALL
SELECT 'AuditLogs',                    COUNT(*)            FROM AuditLogs;
GO


-- ─────────────────────────────────────────────────────────────
-- 7. USEFUL REFERENCE QUERIES
-- ─────────────────────────────────────────────────────────────

-- All purchase bills (summary)
-- SELECT Id, BillNumber, BillDate, Status,
--        TotalItems, TotalQuantity, TotalAmount, CreatedAt
-- FROM PurchaseBills
-- ORDER BY CreatedAt DESC;

-- Full bill detail with line items
-- SELECT b.BillNumber, b.BillDate, b.Status,
--        i.Name        AS Item,
--        l.Code        AS LocationCode,
--        l.Name        AS LocationName,
--        bi.Quantity,
--        bi.Cost,
--        bi.Price,
--        bi.DiscountPercent,
--        bi.TotalCost,
--        bi.TotalSelling
-- FROM PurchaseBills      b
-- JOIN PurchaseBillItems  bi ON bi.PurchaseBillId = b.Id
-- JOIN Items              i  ON i.Id  = bi.ItemId
-- JOIN Locations          l  ON l.Id  = bi.LocationId
-- WHERE b.Id = 1         -- replace with the target bill ID
-- ORDER BY bi.SortOrder;

-- Audit trail for a specific bill
-- SELECT Action, EntityId, CreatedAt, OldValue, NewValue
-- FROM AuditLogs
-- WHERE Entity = 'PurchaseBill' AND EntityId = 1
-- ORDER BY CreatedAt DESC;

-- All audit logs
-- SELECT Id, Entity, Action, EntityId, CreatedAt
-- FROM AuditLogs
-- ORDER BY CreatedAt DESC;
GO
