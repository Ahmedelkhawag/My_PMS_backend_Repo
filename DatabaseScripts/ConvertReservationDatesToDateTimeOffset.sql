BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222195946_ConvertReservationDatesToDateTimeOffset'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reservations]') AND [c].[name] = N'CheckOutDate');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Reservations] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Reservations] ALTER COLUMN [CheckOutDate] datetimeoffset NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222195946_ConvertReservationDatesToDateTimeOffset'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reservations]') AND [c].[name] = N'CheckInDate');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Reservations] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Reservations] ALTER COLUMN [CheckInDate] datetimeoffset NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222195946_ConvertReservationDatesToDateTimeOffset'
)
BEGIN

                    UPDATE Reservations 
                    SET CheckInDate = CAST(CheckInDate as datetime2) AT TIME ZONE 'Egypt Standard Time',
                        CheckOutDate = CAST(CheckOutDate as datetime2) AT TIME ZONE 'Egypt Standard Time';
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260222195946_ConvertReservationDatesToDateTimeOffset'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260222195946_ConvertReservationDatesToDateTimeOffset', N'10.0.2');
END;

COMMIT;
GO

