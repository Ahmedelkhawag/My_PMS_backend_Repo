BEGIN TRANSACTION;
CREATE TABLE [JournalEntryMappings] (
    [Id] int NOT NULL IDENTITY,
    [TransactionType] int NOT NULL,
    [DebitAccountId] int NOT NULL,
    [CreditAccountId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    [LastModifiedAt] datetime2 NULL,
    CONSTRAINT [PK_JournalEntryMappings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JournalEntryMappings_Accounts_CreditAccountId] FOREIGN KEY ([CreditAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_JournalEntryMappings_Accounts_DebitAccountId] FOREIGN KEY ([DebitAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_JournalEntryMappings_CreditAccountId] ON [JournalEntryMappings] ([CreditAccountId]);

CREATE INDEX [IX_JournalEntryMappings_DebitAccountId] ON [JournalEntryMappings] ([DebitAccountId]);

CREATE INDEX [IX_JournalEntryMappings_TransactionType] ON [JournalEntryMappings] ([TransactionType]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260304140553_AddJournalEntryMappingAndAccounting', N'10.0.2');

COMMIT;
GO

