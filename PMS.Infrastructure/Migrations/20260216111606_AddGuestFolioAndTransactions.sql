BEGIN TRANSACTION;
CREATE TABLE [GuestFolios] (
    [Id] int NOT NULL IDENTITY,
    [ReservationId] int NOT NULL,
    [TotalCharges] decimal(18,2) NOT NULL,
    [TotalPayments] decimal(18,2) NOT NULL,
    [Balance] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL,
    [Currency] nvarchar(3) NOT NULL DEFAULT N'EGP',
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_GuestFolios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GuestFolios_Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [Reservations] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [FolioTransactions] (
    [Id] int NOT NULL IDENTITY,
    [FolioId] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [Type] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ReferenceNo] nvarchar(max) NULL,
    [IsVoided] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_FolioTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FolioTransactions_GuestFolios_FolioId] FOREIGN KEY ([FolioId]) REFERENCES [GuestFolios] ([Id]) ON DELETE CASCADE
);

UPDATE [Rooms] SET [HKStatus] = 2
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Rooms] SET [HKStatus] = 2
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Rooms] SET [HKStatus] = 2
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


CREATE INDEX [IX_FolioTransactions_FolioId] ON [FolioTransactions] ([FolioId]);

CREATE UNIQUE INDEX [IX_GuestFolios_ReservationId] ON [GuestFolios] ([ReservationId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260216111606_AddGuestFolioAndTransactions', N'10.0.2');

COMMIT;
GO

