IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [Entity] nvarchar(100) NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [EntityId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE TABLE [Items] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Items] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE TABLE [Locations] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseBills] (
        [Id] int NOT NULL IDENTITY,
        [BillNumber] nvarchar(50) NOT NULL,
        [BillDate] datetime2 NOT NULL,
        [Notes] nvarchar(max) NULL,
        [TotalItems] int NOT NULL,
        [TotalQuantity] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PurchaseBills] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE TABLE [PurchaseBillItems] (
        [Id] int NOT NULL IDENTITY,
        [PurchaseBillId] int NOT NULL,
        [ItemId] int NOT NULL,
        [LocationId] int NOT NULL,
        [Cost] decimal(18,2) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [DiscountPercent] decimal(5,2) NOT NULL,
        [TotalCost] decimal(18,2) NOT NULL,
        [TotalSelling] decimal(18,2) NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_PurchaseBillItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseBillItems_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseBillItems_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PurchaseBillItems_PurchaseBills_PurchaseBillId] FOREIGN KEY ([PurchaseBillId]) REFERENCES [PurchaseBills] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Items]'))
        SET IDENTITY_INSERT [Items] ON;
    EXEC(N'INSERT INTO [Items] ([Id], [CreatedAt], [IsActive], [Name])
    VALUES (1, ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Mango''),
    (2, ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Apple''),
    (3, ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Banana''),
    (4, ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Orange''),
    (5, ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Grapes''),
    (6, ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Kiwi''),
    (7, ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Strawberry'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Items]'))
        SET IDENTITY_INSERT [Items] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Locations]'))
        SET IDENTITY_INSERT [Locations] ON;
    EXEC(N'INSERT INTO [Locations] ([Id], [Code], [CreatedAt], [IsActive], [Name])
    VALUES (1, N''LOC001'', ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Warehouse A''),
    (2, N''LOC002'', ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Warehouse B''),
    (3, N''LOC003'', ''2024-01-01T00:00:00.0000000Z'', CAST(1 AS bit), N''Main Store'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[Locations]'))
        SET IDENTITY_INSERT [Locations] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Items_Name] ON [Items] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Locations_Code] ON [Locations] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseBillItems_ItemId] ON [PurchaseBillItems] ([ItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseBillItems_LocationId] ON [PurchaseBillItems] ([LocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PurchaseBillItems_PurchaseBillId] ON [PurchaseBillItems] ([PurchaseBillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseBills_BillNumber] ON [PurchaseBills] ([BillNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260329050027_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260329050027_InitialCreate', N'9.0.0');
END;

COMMIT;
GO

