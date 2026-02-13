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
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [Countries] (
        [CountryID] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Countries] PRIMARY KEY ([CountryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [Statuses] (
        [StatusID] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Statuses] PRIMARY KEY ([StatusID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [WorkNumber] nvarchar(max) NULL,
        [CountryCode] nvarchar(max) NULL,
        [Gender] int NULL,
        [NationalId] nvarchar(max) NOT NULL,
        [Nationality] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NULL,
        [StatusID] uniqueidentifier NOT NULL,
        [CountryID] nvarchar(450) NULL,
        [DateOfBirth] datetime2 NULL,
        [ProfileImage] nvarchar(max) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUsers_Countries_CountryID] FOREIGN KEY ([CountryID]) REFERENCES [Countries] ([CountryID]),
        CONSTRAINT [FK_AspNetUsers_Statuses_StatusID] FOREIGN KEY ([StatusID]) REFERENCES [Statuses] ([StatusID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_CountryID] ON [AspNetUsers] ([CountryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_StatusID] ON [AspNetUsers] ([StatusID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202120011_FirstMig'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260202120011_FirstMig', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194304_UpdateUserAndAddDocs'
)
BEGIN
    EXEC sp_rename N'[AspNetUsers].[ProfileImage]', N'ProfileImagePath', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194304_UpdateUserAndAddDocs'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [HotelId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194304_UpdateUserAndAddDocs'
)
BEGIN
    CREATE TABLE [EmployeeDocument] (
        [Id] int NOT NULL IDENTITY,
        [FileName] nvarchar(max) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [FileType] nvarchar(max) NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_EmployeeDocument] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmployeeDocument_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194304_UpdateUserAndAddDocs'
)
BEGIN
    CREATE INDEX [IX_EmployeeDocument_AppUserId] ON [EmployeeDocument] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194304_UpdateUserAndAddDocs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260204194304_UpdateUserAndAddDocs', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194638_AddEmployeeDocumentsDbSet'
)
BEGIN
    ALTER TABLE [EmployeeDocument] DROP CONSTRAINT [FK_EmployeeDocument_AspNetUsers_AppUserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194638_AddEmployeeDocumentsDbSet'
)
BEGIN
    ALTER TABLE [EmployeeDocument] DROP CONSTRAINT [PK_EmployeeDocument];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194638_AddEmployeeDocumentsDbSet'
)
BEGIN
    EXEC sp_rename N'[EmployeeDocument]', N'EmployeeDocuments', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194638_AddEmployeeDocumentsDbSet'
)
BEGIN
    EXEC sp_rename N'[EmployeeDocuments].[IX_EmployeeDocument_AppUserId]', N'IX_EmployeeDocuments_AppUserId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194638_AddEmployeeDocumentsDbSet'
)
BEGIN
    ALTER TABLE [EmployeeDocuments] ADD CONSTRAINT [PK_EmployeeDocuments] PRIMARY KEY ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194638_AddEmployeeDocumentsDbSet'
)
BEGIN
    ALTER TABLE [EmployeeDocuments] ADD CONSTRAINT [FK_EmployeeDocuments_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204194638_AddEmployeeDocumentsDbSet'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260204194638_AddEmployeeDocumentsDbSet', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204200234_UpdateUserAndAddingChangepassApproveAndIsActiveProp'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ChangePasswordApprove] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204200234_UpdateUserAndAddingChangepassApproveAndIsActiveProp'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204200234_UpdateUserAndAddingChangepassApproveAndIsActiveProp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260204200234_UpdateUserAndAddingChangepassApproveAndIsActiveProp', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204210201_MakeStatusIdNullable'
)
BEGIN
    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Statuses_StatusID];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204210201_MakeStatusIdNullable'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'StatusID');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [StatusID] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204210201_MakeStatusIdNullable'
)
BEGIN
    UPDATE AspNetUsers SET StatusID = NULL WHERE StatusID = '00000000-0000-0000-0000-000000000000'
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204210201_MakeStatusIdNullable'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Statuses_StatusID] FOREIGN KEY ([StatusID]) REFERENCES [Statuses] ([StatusID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204210201_MakeStatusIdNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260204210201_MakeStatusIdNullable', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204224959_AddSoftDeleteColumns'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204224959_AddSoftDeleteColumns'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DeletedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204224959_AddSoftDeleteColumns'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260204224959_AddSoftDeleteColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260204224959_AddSoftDeleteColumns', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207103513_AddRefreshTokensTable'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [Token] nvarchar(max) NOT NULL,
        [ExpiresOn] datetime2 NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [RevokedOn] datetime2 NULL,
        [AppUserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207103513_AddRefreshTokensTable'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_AppUserId] ON [RefreshTokens] ([AppUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207103513_AddRefreshTokensTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260207103513_AddRefreshTokensTable', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207205952_AddRoomsTables'
)
BEGIN
    CREATE TABLE [RoomTypes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [BasePrice] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NULL,
        [MaxAdults] int NOT NULL,
        [MaxChildren] int NOT NULL,
        CONSTRAINT [PK_RoomTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207205952_AddRoomsTables'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] int NOT NULL IDENTITY,
        [RoomNumber] nvarchar(max) NOT NULL,
        [FloorNumber] int NOT NULL,
        [Status] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [RoomTypeId] int NOT NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Rooms_RoomTypes_RoomTypeId] FOREIGN KEY ([RoomTypeId]) REFERENCES [RoomTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207205952_AddRoomsTables'
)
BEGIN
    CREATE INDEX [IX_Rooms_RoomTypeId] ON [Rooms] ([RoomTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207205952_AddRoomsTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260207205952_AddRoomsTables', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207210500_SeedRoomsData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BasePrice', N'Description', N'MaxAdults', N'MaxChildren', N'Name') AND [object_id] = OBJECT_ID(N'[RoomTypes]'))
        SET IDENTITY_INSERT [RoomTypes] ON;
    EXEC(N'INSERT INTO [RoomTypes] ([Id], [BasePrice], [Description], [MaxAdults], [MaxChildren], [Name])
    VALUES (1, 250.0, N''غرفة لشخص واحد'', 1, 0, N''فردية''),
    (2, 350.0, N''غرفة لشخصين'', 2, 1, N''مزدوجة''),
    (3, 540.0, N''جناح فاخر'', 2, 2, N''جناح''),
    (4, 500.0, N''غرفة مميزة بإطلالة'', 2, 1, N''ديلوكس'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BasePrice', N'Description', N'MaxAdults', N'MaxChildren', N'Name') AND [object_id] = OBJECT_ID(N'[RoomTypes]'))
        SET IDENTITY_INSERT [RoomTypes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207210500_SeedRoomsData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FloorNumber', N'IsActive', N'Notes', N'RoomNumber', N'RoomTypeId', N'Status') AND [object_id] = OBJECT_ID(N'[Rooms]'))
        SET IDENTITY_INSERT [Rooms] ON;
    EXEC(N'INSERT INTO [Rooms] ([Id], [FloorNumber], [IsActive], [Notes], [RoomNumber], [RoomTypeId], [Status])
    VALUES (1, 1, CAST(1 AS bit), NULL, N''101'', 1, 0),
    (2, 1, CAST(1 AS bit), NULL, N''102'', 2, 1),
    (3, 1, CAST(1 AS bit), NULL, N''103'', 2, 3),
    (4, 2, CAST(1 AS bit), NULL, N''201'', 3, 0),
    (5, 2, CAST(1 AS bit), NULL, N''202'', 4, 2),
    (6, 2, CAST(1 AS bit), NULL, N''203'', 2, 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'FloorNumber', N'IsActive', N'Notes', N'RoomNumber', N'RoomTypeId', N'Status') AND [object_id] = OBJECT_ID(N'[Rooms]'))
        SET IDENTITY_INSERT [Rooms] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260207210500_SeedRoomsData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260207210500_SeedRoomsData', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260208123644_AddGuestsTable'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260208123644_AddGuestsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260208123644_AddGuestsTable', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260208125838_UpdateGuestsTable'
)
BEGIN
    ALTER TABLE [Guests] ADD [DateOfBirth] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260208125838_UpdateGuestsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260208125838_UpdateGuestsTable', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209102211_AddReservationWithServices'
)
BEGIN
    CREATE TABLE [Reservations] (
        [Id] int NOT NULL IDENTITY,
        [ReservationNumber] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [GuestId] int NOT NULL,
        [RoomId] int NULL,
        [RoomTypeId] int NOT NULL,
        [CheckInDate] datetime2 NOT NULL,
        [CheckOutDate] datetime2 NOT NULL,
        [NightlyRate] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [ServicesAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [GrandTotal] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [Source] int NOT NULL,
        [RateCode] nvarchar(max) NOT NULL,
        [MealPlan] nvarchar(max) NOT NULL,
        [IsPostMaster] bit NOT NULL,
        [IsNoExtend] bit NOT NULL,
        [IsGuestPay] bit NOT NULL,
        [Adults] int NOT NULL,
        [Children] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_Reservations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reservations_Guests_GuestId] FOREIGN KEY ([GuestId]) REFERENCES [Guests] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reservations_RoomTypes_RoomTypeId] FOREIGN KEY ([RoomTypeId]) REFERENCES [RoomTypes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Reservations_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209102211_AddReservationWithServices'
)
BEGIN
    CREATE TABLE [ReservationServices] (
        [Id] int NOT NULL IDENTITY,
        [ReservationId] int NOT NULL,
        [ServiceName] nvarchar(max) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [IsPerDay] bit NOT NULL,
        [TotalServicePrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ReservationServices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReservationServices_Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [Reservations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209102211_AddReservationWithServices'
)
BEGIN
    CREATE INDEX [IX_Reservations_GuestId] ON [Reservations] ([GuestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209102211_AddReservationWithServices'
)
BEGIN
    CREATE INDEX [IX_Reservations_RoomId] ON [Reservations] ([RoomId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209102211_AddReservationWithServices'
)
BEGIN
    CREATE INDEX [IX_Reservations_RoomTypeId] ON [Reservations] ([RoomTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209102211_AddReservationWithServices'
)
BEGIN
    CREATE INDEX [IX_ReservationServices_ReservationId] ON [ReservationServices] ([ReservationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209102211_AddReservationWithServices'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209102211_AddReservationWithServices', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209103347_AddReservationBusinessFields'
)
BEGIN
    ALTER TABLE [Reservations] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209103347_AddReservationBusinessFields'
)
BEGIN
    ALTER TABLE [Reservations] ADD [MarketSegment] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209103347_AddReservationBusinessFields'
)
BEGIN
    ALTER TABLE [Reservations] ADD [PurposeOfVisit] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209103347_AddReservationBusinessFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209103347_AddReservationBusinessFields', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209115458_AddExternalRefAndCarPlate'
)
BEGIN
    ALTER TABLE [Reservations] ADD [CarPlate] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209115458_AddExternalRefAndCarPlate'
)
BEGIN
    ALTER TABLE [Reservations] ADD [ExternalReference] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209115458_AddExternalRefAndCarPlate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209115458_AddExternalRefAndCarPlate', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209120439_AddSoftDeleteToReservation'
)
BEGIN
    ALTER TABLE [Reservations] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209120439_AddSoftDeleteToReservation'
)
BEGIN
    ALTER TABLE [Reservations] ADD [DeletedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209120439_AddSoftDeleteToReservation'
)
BEGIN
    ALTER TABLE [Reservations] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209120439_AddSoftDeleteToReservation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209120439_AddSoftDeleteToReservation', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124245_AddAuditingFields'
)
BEGIN
    ALTER TABLE [Reservations] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124245_AddAuditingFields'
)
BEGIN
    ALTER TABLE [Reservations] ADD [LastModifiedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124245_AddAuditingFields'
)
BEGIN
    ALTER TABLE [Reservations] ADD [LastModifiedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124245_AddAuditingFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209124245_AddAuditingFields', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Rooms] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Rooms] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Rooms] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Rooms] ADD [DeletedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Rooms] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Rooms] ADD [LastModifiedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Rooms] ADD [LastModifiedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [ReservationServices] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [ReservationServices] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [ReservationServices] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [ReservationServices] ADD [DeletedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [ReservationServices] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [ReservationServices] ADD [LastModifiedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [ReservationServices] ADD [LastModifiedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [DeletedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [LastModifiedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [RefreshTokens] ADD [LastModifiedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Guests] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Guests] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Guests] ADD [DeletedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Guests] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Guests] ADD [LastModifiedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [Guests] ADD [LastModifiedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastModifiedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastModifiedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''0001-01-01T00:00:00.0000000'', [CreatedBy] = NULL, [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit), [LastModifiedAt] = NULL, [LastModifiedBy] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''0001-01-01T00:00:00.0000000'', [CreatedBy] = NULL, [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit), [LastModifiedAt] = NULL, [LastModifiedBy] = NULL
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''0001-01-01T00:00:00.0000000'', [CreatedBy] = NULL, [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit), [LastModifiedAt] = NULL, [LastModifiedBy] = NULL
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''0001-01-01T00:00:00.0000000'', [CreatedBy] = NULL, [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit), [LastModifiedAt] = NULL, [LastModifiedBy] = NULL
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''0001-01-01T00:00:00.0000000'', [CreatedBy] = NULL, [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit), [LastModifiedAt] = NULL, [LastModifiedBy] = NULL
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''0001-01-01T00:00:00.0000000'', [CreatedBy] = NULL, [DeletedAt] = NULL, [DeletedBy] = NULL, [IsDeleted] = CAST(0 AS bit), [LastModifiedAt] = NULL, [LastModifiedBy] = NULL
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209124654_AddAuditingFields2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209124654_AddAuditingFields2', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    CREATE TABLE [BookingSources] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_BookingSources] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    CREATE TABLE [MarketSegments] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_MarketSegments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    CREATE TABLE [MealPlans] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_MealPlans] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    CREATE TABLE [RoomStatusLookups] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Color] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_RoomStatusLookups] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[BookingSources]'))
        SET IDENTITY_INSERT [BookingSources] ON;
    EXEC(N'INSERT INTO [BookingSources] ([Id], [IsActive], [Name])
    VALUES (1, CAST(1 AS bit), N''Direct (Walk-in)''),
    (2, CAST(1 AS bit), N''Phone''),
    (3, CAST(1 AS bit), N''Booking.com''),
    (4, CAST(1 AS bit), N''Expedia''),
    (5, CAST(1 AS bit), N''Website'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[BookingSources]'))
        SET IDENTITY_INSERT [BookingSources] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[MarketSegments]'))
        SET IDENTITY_INSERT [MarketSegments] ON;
    EXEC(N'INSERT INTO [MarketSegments] ([Id], [IsActive], [Name])
    VALUES (1, CAST(1 AS bit), N''Individual (أفراد)''),
    (2, CAST(1 AS bit), N''Corporate (شركات)''),
    (3, CAST(1 AS bit), N''Group (مجموعات)''),
    (4, CAST(1 AS bit), N''Government (حكومي)'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[MarketSegments]'))
        SET IDENTITY_INSERT [MarketSegments] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'Price') AND [object_id] = OBJECT_ID(N'[MealPlans]'))
        SET IDENTITY_INSERT [MealPlans] ON;
    EXEC(N'INSERT INTO [MealPlans] ([Id], [IsActive], [Name], [Price])
    VALUES (1, CAST(1 AS bit), N''Room Only (بدون وجبات)'', 0.0),
    (2, CAST(1 AS bit), N''Bed & Breakfast (إفطار)'', 150.0),
    (3, CAST(1 AS bit), N''Half Board (إفطار وعشاء)'', 400.0),
    (4, CAST(1 AS bit), N''Full Board (إفطار وغداء وعشاء)'', 700.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsActive', N'Name', N'Price') AND [object_id] = OBJECT_ID(N'[MealPlans]'))
        SET IDENTITY_INSERT [MealPlans] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Color', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[RoomStatusLookups]'))
        SET IDENTITY_INSERT [RoomStatusLookups] ON;
    EXEC(N'INSERT INTO [RoomStatusLookups] ([Id], [Color], [IsActive], [Name])
    VALUES (1, N''#28A745'', CAST(1 AS bit), N''Clean (نظيفة)''),
    (2, N''#DC3545'', CAST(1 AS bit), N''Dirty (متسخة)''),
    (3, N''#FFC107'', CAST(1 AS bit), N''Maintenance (صيانة)''),
    (4, N''#6C757D'', CAST(1 AS bit), N''Out of Order (خارج الخدمة)'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Color', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[RoomStatusLookups]'))
        SET IDENTITY_INSERT [RoomStatusLookups] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209134009_AddLookupTablesWithSeeding'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209134009_AddLookupTablesWithSeeding', N'10.0.2');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reservations]') AND [c].[name] = N'MarketSegment');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Reservations] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Reservations] DROP COLUMN [MarketSegment];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reservations]') AND [c].[name] = N'MealPlan');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Reservations] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Reservations] DROP COLUMN [MealPlan];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC sp_rename N'[Rooms].[Status]', N'RoomStatusId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC sp_rename N'[Reservations].[Source]', N'MealPlanId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    ALTER TABLE [Reservations] ADD [BookingSourceId] int NOT NULL DEFAULT 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    ALTER TABLE [Reservations] ADD [MarketSegmentId] int NOT NULL DEFAULT 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000'', [RoomStatusId] = 1
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000'', [RoomStatusId] = 2
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000'', [RoomStatusId] = 1
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000'', [RoomStatusId] = 1
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000'', [RoomStatusId] = 3
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC(N'UPDATE [Rooms] SET [CreatedAt] = ''2026-01-01T00:00:00.0000000'', [RoomStatusId] = 4
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    CREATE INDEX [IX_Rooms_RoomStatusId] ON [Rooms] ([RoomStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    CREATE INDEX [IX_Reservations_BookingSourceId] ON [Reservations] ([BookingSourceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    CREATE INDEX [IX_Reservations_MarketSegmentId] ON [Reservations] ([MarketSegmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    CREATE INDEX [IX_Reservations_MealPlanId] ON [Reservations] ([MealPlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    EXEC('UPDATE Rooms SET RoomStatusId = 1 WHERE RoomStatusId = 0 OR RoomStatusId IS NULL')
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    ALTER TABLE [Reservations] ADD CONSTRAINT [FK_Reservations_BookingSources_BookingSourceId] FOREIGN KEY ([BookingSourceId]) REFERENCES [BookingSources] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    ALTER TABLE [Reservations] ADD CONSTRAINT [FK_Reservations_MarketSegments_MarketSegmentId] FOREIGN KEY ([MarketSegmentId]) REFERENCES [MarketSegments] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    ALTER TABLE [Reservations] ADD CONSTRAINT [FK_Reservations_MealPlans_MealPlanId] FOREIGN KEY ([MealPlanId]) REFERENCES [MealPlans] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    ALTER TABLE [Rooms] ADD CONSTRAINT [FK_Rooms_RoomStatusLookups_RoomStatusId] FOREIGN KEY ([RoomStatusId]) REFERENCES [RoomStatusLookups] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260209144024_LinkRoomWithStatusLookup'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260209144024_LinkRoomWithStatusLookup', N'10.0.2');
END;

COMMIT;
GO

