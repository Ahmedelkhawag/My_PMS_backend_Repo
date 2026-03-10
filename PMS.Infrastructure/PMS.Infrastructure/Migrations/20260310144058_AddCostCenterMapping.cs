using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCostCenterMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "JournalEntryMappings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryMappings_CostCenterId",
                table: "JournalEntryMappings",
                column: "CostCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntryMappings_CostCenters_CostCenterId",
                table: "JournalEntryMappings",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntryMappings_CostCenters_CostCenterId",
                table: "JournalEntryMappings");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntryMappings_CostCenterId",
                table: "JournalEntryMappings");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "JournalEntryMappings");
        }
    }
}
