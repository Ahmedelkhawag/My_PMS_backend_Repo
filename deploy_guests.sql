BEGIN TRANSACTION;
CREATE TABLE [Guests] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NOT NULL,
    [NationalId] nvarchar(max) NOT NULL,
    [Nationality] nvarchar(max) NOT NULL,
    [LoyaltyLevel] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [Email] nvarchar(100) NULL,
    [Address] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [CarNumber] nvarchar(20) NULL,
    [VatNumber] nvarchar(50) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Guests] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260208123644_AddGuestsTable', N'10.0.2');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Guests] ADD [DateOfBirth] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260208125838_UpdateGuestsTable', N'10.0.2');

COMMIT;
GO

