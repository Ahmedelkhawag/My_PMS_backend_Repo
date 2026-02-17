BEGIN TRANSACTION;
DROP INDEX [IX_EmployeeShifts_EmployeeId] ON [EmployeeShifts];

CREATE UNIQUE INDEX [IX_EmployeeShifts_EmployeeId_IsClosed] ON [EmployeeShifts] ([EmployeeId], [IsClosed]) WHERE [IsClosed] = 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260217112017_AddOpenShiftFilteredUniqueIndex', N'10.0.2');

COMMIT;
GO

