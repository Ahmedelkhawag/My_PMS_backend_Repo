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
            migrationBuilder.AddColumn<bool>(
                name: "RequiresExternalReference",
                table: "BookingSources",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
