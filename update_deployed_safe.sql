-- Safe script: adds columns only if missing, then updates and records migrations.
-- Run this on the deployed database. Safe to run multiple times.

-- ========== Migration 1: AddRoomFOAndHKStatus ==========
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'HKStatus')
BEGIN
    ALTER TABLE [Rooms] ADD [HKStatus] int NOT NULL DEFAULT 2;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'BedType')
BEGIN
    ALTER TABLE [Rooms] ADD [BedType] int NOT NULL DEFAULT 1;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'ViewType')
BEGIN
    ALTER TABLE [Rooms] ADD [ViewType] nvarchar(max) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'MaxAdults')
BEGIN
    ALTER TABLE [Rooms] ADD [MaxAdults] int NOT NULL DEFAULT 0;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'BasePrice')
BEGIN
    ALTER TABLE [Rooms] ADD [BasePrice] decimal(18,2) NOT NULL DEFAULT 0.0;
END
GO

-- Fill HKStatus from RoomStatusLookups (only if column exists)
UPDATE Rooms
SET HKStatus = CASE
    WHEN RoomStatusId IN (SELECT Id FROM RoomStatusLookups WHERE Name LIKE N'Clean%') THEN 1
    ELSE 2
END
WHERE 1=1;
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260215100000_AddRoomFOAndHKStatus')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260215100000_AddRoomFOAndHKStatus', N'10.0.2');
END
GO

-- ========== Migration 2: AddRoomMaintenanceFields ==========
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'MaintenanceReason')
BEGIN
    ALTER TABLE [Rooms] ADD [MaintenanceReason] nvarchar(max) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'MaintenanceStartDate')
BEGIN
    ALTER TABLE [Rooms] ADD [MaintenanceStartDate] datetime2 NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'MaintenanceEndDate')
BEGIN
    ALTER TABLE [Rooms] ADD [MaintenanceEndDate] datetime2 NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Rooms') AND name = 'MaintenanceRemarks')
BEGIN
    ALTER TABLE [Rooms] ADD [MaintenanceRemarks] nvarchar(max) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260216000000_AddRoomMaintenanceFields')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260216000000_AddRoomMaintenanceFields', N'10.0.2');
END
GO
