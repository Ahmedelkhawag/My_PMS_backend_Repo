using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoomStatusLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RoomStatusLookups",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Color", "Name" },
                values: new object[] { "#2ecc71", "Inspected (تم الفحص)" });

            migrationBuilder.UpdateData(
                table: "RoomStatusLookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Out of Order (صيانة جسيمة)");

            migrationBuilder.UpdateData(
                table: "RoomStatusLookups",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Color", "Name" },
                values: new object[] { "#FFC107", "Out of Service (صيانة بسيطة)" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RoomStatusLookups",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Color", "Name" },
                values: new object[] { "#FFC107", "Maintenance (صيانة)" });

            migrationBuilder.UpdateData(
                table: "RoomStatusLookups",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Out of Order (خارج الخدمة)");

            migrationBuilder.UpdateData(
                table: "RoomStatusLookups",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Color", "Name" },
                values: new object[] { "#17A2B8", "Occupied" });
        }
    }
}
