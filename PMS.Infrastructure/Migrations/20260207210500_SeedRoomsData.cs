using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoomsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RoomTypes",
                columns: new[] { "Id", "BasePrice", "Description", "MaxAdults", "MaxChildren", "Name" },
                values: new object[,]
                {
                    { 1, 250m, "غرفة لشخص واحد", 1, 0, "فردية" },
                    { 2, 350m, "غرفة لشخصين", 2, 1, "مزدوجة" },
                    { 3, 540m, "جناح فاخر", 2, 2, "جناح" },
                    { 4, 500m, "غرفة مميزة بإطلالة", 2, 1, "ديلوكس" }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "FloorNumber", "IsActive", "Notes", "RoomNumber", "RoomTypeId", "Status" },
                values: new object[,]
                {
                    { 1, 1, true, null, "101", 1, 0 },
                    { 2, 1, true, null, "102", 2, 1 },
                    { 3, 1, true, null, "103", 2, 3 },
                    { 4, 2, true, null, "201", 3, 0 },
                    { 5, 2, true, null, "202", 4, 2 },
                    { 6, 2, true, null, "203", 2, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RoomTypes",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
