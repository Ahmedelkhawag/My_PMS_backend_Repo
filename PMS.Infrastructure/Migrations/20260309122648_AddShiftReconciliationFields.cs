using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftReconciliationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FolioId",
                table: "FolioTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ReconciliationTransactionId",
                table: "EmployeeShifts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShifts_ReconciliationTransactionId",
                table: "EmployeeShifts",
                column: "ReconciliationTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShifts_FolioTransactions_ReconciliationTransactionId",
                table: "EmployeeShifts",
                column: "ReconciliationTransactionId",
                principalTable: "FolioTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShifts_FolioTransactions_ReconciliationTransactionId",
                table: "EmployeeShifts");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeShifts_ReconciliationTransactionId",
                table: "EmployeeShifts");

            migrationBuilder.DropColumn(
                name: "ReconciliationTransactionId",
                table: "EmployeeShifts");

            migrationBuilder.AlterColumn<int>(
                name: "FolioId",
                table: "FolioTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
