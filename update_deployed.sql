BEGIN TRANSACTION;
ALTER TABLE [Rooms] ADD [HKStatus] int NOT NULL DEFAULT 2;

ALTER TABLE [Rooms] ADD [BedType] int NOT NULL DEFAULT 1;

ALTER TABLE [Rooms] ADD [ViewType] nvarchar(max) NULL;

ALTER TABLE [Rooms] ADD [MaxAdults] int NOT NULL DEFAULT 0;

ALTER TABLE [Rooms] ADD [BasePrice] decimal(18,2) NOT NULL DEFAULT 0.0;


                UPDATE Rooms
                SET HKStatus = CASE
                    WHEN RoomStatusId IN (SELECT Id FROM RoomStatusLookups WHERE Name LIKE N'Clean%') THEN 1
                    ELSE 2
                END
            

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260215100000_AddRoomFOAndHKStatus', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Rooms] ADD [MaintenanceReason] nvarchar(max) NULL;

ALTER TABLE [Rooms] ADD [MaintenanceStartDate] datetime2 NULL;

ALTER TABLE [Rooms] ADD [MaintenanceEndDate] datetime2 NULL;

ALTER TABLE [Rooms] ADD [MaintenanceRemarks] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260216000000_AddRoomMaintenanceFields', N'10.0.2');

COMMIT;
GO

