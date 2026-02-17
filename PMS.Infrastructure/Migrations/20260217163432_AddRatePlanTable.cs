using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRatePlanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRateOverridden",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegacyRateCode",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatePlanId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatePlanId",
                table: "CompanyProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RatePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RateType = table.Column<int>(type: "int", nullable: false),
                    RateValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatePlans", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RatePlans",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "IsDeleted", "IsPublic", "LastModifiedAt", "LastModifiedBy", "Name", "RateType", "RateValue" },
                values: new object[,]
                {
                    { 1, "STANDARD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System", null, null, "Standard public rate plan (no discount).", true, false, true, null, null, "Standard Rate", 2, 0m },
                    { 2, "NONREF", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System", null, null, "Non-refundable rate with 10% discount.", true, false, true, null, null, "Non-Refundable", 2, 10m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RatePlanId",
                table: "Reservations",
                column: "RatePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProfiles_RatePlanId",
                table: "CompanyProfiles",
                column: "RatePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_RatePlans_Code",
                table: "RatePlans",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyProfiles_RatePlans_RatePlanId",
                table: "CompanyProfiles",
                column: "RatePlanId",
                principalTable: "RatePlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_RatePlans_RatePlanId",
                table: "Reservations",
                column: "RatePlanId",
                principalTable: "RatePlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyProfiles_RatePlans_RatePlanId",
                table: "CompanyProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_RatePlans_RatePlanId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "RatePlans");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_RatePlanId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_CompanyProfiles_RatePlanId",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "IsRateOverridden",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "LegacyRateCode",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RatePlanId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RatePlanId",
                table: "CompanyProfiles");
        }
    }
}
