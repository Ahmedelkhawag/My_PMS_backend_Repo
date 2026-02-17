BEGIN TRANSACTION;
ALTER TABLE [FolioTransactions] ADD [ShiftId] int NULL;

CREATE TABLE [EmployeeShifts] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] nvarchar(450) NOT NULL,
    [StartedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [EndedAt] datetime2 NULL,
    [StartingCash] decimal(18,2) NOT NULL,
    [SystemCalculatedCash] decimal(18,2) NOT NULL,
    [ActualCashHanded] decimal(18,2) NULL,
    [Difference] decimal(18,2) NULL,
    [Notes] nvarchar(max) NOT NULL,
    [IsClosed] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_EmployeeShifts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmployeeShifts_AspNetUsers_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_FolioTransactions_ShiftId] ON [FolioTransactions] ([ShiftId]);

CREATE INDEX [IX_EmployeeShifts_EmployeeId] ON [EmployeeShifts] ([EmployeeId]);

ALTER TABLE [FolioTransactions] ADD CONSTRAINT [FK_FolioTransactions_EmployeeShifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [EmployeeShifts] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260217103657_AddEmployeeShiftTable', N'10.0.2');

COMMIT;
GO

