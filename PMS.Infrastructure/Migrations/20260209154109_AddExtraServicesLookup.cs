using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraServicesLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtraService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPerDay = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraService", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ExtraService",
                columns: new[] { "Id", "IsActive", "IsPerDay", "Name", "Price" },
                values: new object[,]
                {
                    { 1, true, false, "Airport Transfer (نقل مطار)", 150m },
                    { 2, true, true, "Parking (موقف سيارات)", 30m },
                    { 3, true, true, "VIP Service (خدمة VIP)", 200m },
                    { 4, true, false, "Spa (سبا)", 300m },
                    { 5, true, false, "Laundry (غسيل)", 75m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtraService");
        }
    }
}
