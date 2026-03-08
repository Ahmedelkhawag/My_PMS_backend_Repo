using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiresExternalReferenceToBookingSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: add column only if it does not exist (e.g. Test DB already has it)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('BookingSources') AND name = 'RequiresExternalReference'
                )
                BEGIN
                    ALTER TABLE [BookingSources] ADD [RequiresExternalReference] bit NOT NULL DEFAULT CAST(0 AS bit);
                END
            ");

            migrationBuilder.UpdateData(
                table: "BookingSources",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiresExternalReference",
                value: false);

            migrationBuilder.UpdateData(
                table: "BookingSources",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequiresExternalReference",
                value: false);

            migrationBuilder.UpdateData(
                table: "BookingSources",
                keyColumn: "Id",
                keyValue: 3,
                column: "RequiresExternalReference",
                value: true);

            migrationBuilder.UpdateData(
                table: "BookingSources",
                keyColumn: "Id",
                keyValue: 4,
                column: "RequiresExternalReference",
                value: true);

            migrationBuilder.UpdateData(
                table: "BookingSources",
                keyColumn: "Id",
                keyValue: 5,
                column: "RequiresExternalReference",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresExternalReference",
                table: "BookingSources");
        }
    }
}
