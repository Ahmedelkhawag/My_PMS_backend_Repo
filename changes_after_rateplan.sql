BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218101826_AddingToGuestTablePassportNumber'
)
BEGIN
    ALTER TABLE [Guests] ADD [PassportNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218101826_AddingToGuestTablePassportNumber'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218101826_AddingToGuestTablePassportNumber', N'10.0.2');
END;

COMMIT;
GO

