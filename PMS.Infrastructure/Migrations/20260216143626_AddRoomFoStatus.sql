BEGIN TRANSACTION;
ALTER TABLE [Rooms] ADD [FOStatus] int NOT NULL DEFAULT 0;

UPDATE [Rooms] SET [FOStatus] = 1
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Rooms] SET [FOStatus] = 1
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Rooms] SET [FOStatus] = 1
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Rooms] SET [FOStatus] = 1
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Rooms] SET [FOStatus] = 1
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Rooms] SET [FOStatus] = 1
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260216143626_AddRoomFoStatus', N'10.0.2');

COMMIT;
GO

