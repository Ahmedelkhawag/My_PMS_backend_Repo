using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupiedRoomStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RoomStatusLookups",
                columns: new[] { "Id", "Color", "IsActive", "Name" },
                values: new object[] { 5, "#17A2B8", true, "Occupied" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoomStatusLookups",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
