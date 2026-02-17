using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenShiftFilteredUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeShifts_EmployeeId",
                table: "EmployeeShifts");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShifts_EmployeeId_IsClosed",
                table: "EmployeeShifts",
                columns: new[] { "EmployeeId", "IsClosed" },
                unique: true,
                filter: "[IsClosed] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeShifts_EmployeeId_IsClosed",
                table: "EmployeeShifts");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShifts_EmployeeId",
                table: "EmployeeShifts",
                column: "EmployeeId");
        }
    }
}
