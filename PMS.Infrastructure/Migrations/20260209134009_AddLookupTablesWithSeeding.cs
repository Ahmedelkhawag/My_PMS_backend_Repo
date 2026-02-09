using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTablesWithSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSegments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MealPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomStatusLookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomStatusLookups", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BookingSources",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, true, "Direct (Walk-in)" },
                    { 2, true, "Phone" },
                    { 3, true, "Booking.com" },
                    { 4, true, "Expedia" },
                    { 5, true, "Website" }
                });

            migrationBuilder.InsertData(
                table: "MarketSegments",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, true, "Individual (أفراد)" },
                    { 2, true, "Corporate (شركات)" },
                    { 3, true, "Group (مجموعات)" },
                    { 4, true, "Government (حكومي)" }
                });

            migrationBuilder.InsertData(
                table: "MealPlans",
                columns: new[] { "Id", "IsActive", "Name", "Price" },
                values: new object[,]
                {
                    { 1, true, "Room Only (بدون وجبات)", 0m },
                    { 2, true, "Bed & Breakfast (إفطار)", 150m },
                    { 3, true, "Half Board (إفطار وعشاء)", 400m },
                    { 4, true, "Full Board (إفطار وغداء وعشاء)", 700m }
                });

            migrationBuilder.InsertData(
                table: "RoomStatusLookups",
                columns: new[] { "Id", "Color", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "green", true, "Clean (نظيفة)" },
                    { 2, "red", true, "Dirty (متسخة)" },
                    { 3, "orange", true, "Maintenance (صيانة)" },
                    { 4, "gray", true, "Out of Order (خارج الخدمة)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingSources");

            migrationBuilder.DropTable(
                name: "MarketSegments");

            migrationBuilder.DropTable(
                name: "MealPlans");

            migrationBuilder.DropTable(
                name: "RoomStatusLookups");
        }
    }
}
