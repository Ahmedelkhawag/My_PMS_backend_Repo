-- =============================================================================
-- Deploy Script: Migrations بعد 20260304140553_AddJournalEntryMappingAndAccounting
-- =============================================================================
-- استخدم هذا السكريبت على قاعدة البيانات المنشورة (Production/Deploy)
-- آخر migration مطبقة على الديبلوي: 20260304140553_AddJournalEntryMappingAndAccounting
--
-- Migrations المطبقة في هذا السكريبت:
--   1. 20260305113704_AddBackOfficeARFoundation (ARInvoices, ARPayments, ARInvoiceLines, ARPaymentAllocations)
--   2. 20260308110722_AddARPaymentRemarks (عمود Remarks في ARPayments)
--   3. 20260308115522_AddARAdjustments (جدول ARAdjustments)
--
-- السكريبت Idempotent: آمن التشغيل مرات متعددة (يتحقق من __EFMigrationsHistory)
-- =============================================================================

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE TABLE [ARInvoices] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceNumber] nvarchar(50) NOT NULL,
        [CompanyId] int NOT NULL,
        [InvoiceDate] datetime2 NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [PaidAmount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_ARInvoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ARInvoices_CompanyProfiles_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [CompanyProfiles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE TABLE [ARPayments] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [PaymentMethod] nvarchar(50) NOT NULL,
        [ReferenceNumber] nvarchar(100) NULL,
        [UnallocatedAmount] decimal(18,2) NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_ARPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ARPayments_CompanyProfiles_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [CompanyProfiles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE TABLE [ARInvoiceLines] (
        [Id] int NOT NULL IDENTITY,
        [ARInvoiceId] int NOT NULL,
        [FolioTransactionId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_ARInvoiceLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ARInvoiceLines_ARInvoices_ARInvoiceId] FOREIGN KEY ([ARInvoiceId]) REFERENCES [ARInvoices] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ARInvoiceLines_FolioTransactions_FolioTransactionId] FOREIGN KEY ([FolioTransactionId]) REFERENCES [FolioTransactions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE TABLE [ARPaymentAllocations] (
        [Id] int NOT NULL IDENTITY,
        [ARPaymentId] int NOT NULL,
        [ARInvoiceId] int NOT NULL,
        [AmountApplied] decimal(18,2) NOT NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_ARPaymentAllocations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ARPaymentAllocations_ARInvoices_ARInvoiceId] FOREIGN KEY ([ARInvoiceId]) REFERENCES [ARInvoices] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ARPaymentAllocations_ARPayments_ARPaymentId] FOREIGN KEY ([ARPaymentId]) REFERENCES [ARPayments] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE INDEX [IX_ARInvoiceLines_ARInvoiceId] ON [ARInvoiceLines] ([ARInvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE INDEX [IX_ARInvoiceLines_FolioTransactionId] ON [ARInvoiceLines] ([FolioTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE INDEX [IX_ARInvoices_CompanyId] ON [ARInvoices] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ARInvoices_InvoiceNumber] ON [ARInvoices] ([InvoiceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE INDEX [IX_ARPaymentAllocations_ARInvoiceId] ON [ARPaymentAllocations] ([ARInvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE INDEX [IX_ARPaymentAllocations_ARPaymentId] ON [ARPaymentAllocations] ([ARPaymentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    CREATE INDEX [IX_ARPayments_CompanyId] ON [ARPayments] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305113704_AddBackOfficeARFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260305113704_AddBackOfficeARFoundation', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260308110722_AddARPaymentRemarks'
)
BEGIN
    ALTER TABLE [ARPayments] ADD [Remarks] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260308110722_AddARPaymentRemarks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260308110722_AddARPaymentRemarks', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260308115522_AddARAdjustments'
)
BEGIN
    CREATE TABLE [ARAdjustments] (
        [Id] int NOT NULL IDENTITY,
        [ARInvoiceId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Type] int NOT NULL,
        [AdjustmentDate] datetime2 NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [ReferenceNumber] nvarchar(50) NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastModifiedBy] nvarchar(max) NULL,
        [LastModifiedAt] datetime2 NULL,
        CONSTRAINT [PK_ARAdjustments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ARAdjustments_ARInvoices_ARInvoiceId] FOREIGN KEY ([ARInvoiceId]) REFERENCES [ARInvoices] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260308115522_AddARAdjustments'
)
BEGIN
    CREATE INDEX [IX_ARAdjustments_ARInvoiceId] ON [ARAdjustments] ([ARInvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260308115522_AddARAdjustments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260308115522_AddARAdjustments', N'10.0.2');
END;

COMMIT;
GO
