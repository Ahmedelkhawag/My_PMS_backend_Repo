using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeDocumentsDbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocument_AspNetUsers_AppUserId",
                table: "EmployeeDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeDocument",
                table: "EmployeeDocument");

            migrationBuilder.RenameTable(
                name: "EmployeeDocument",
                newName: "EmployeeDocuments");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeDocument_AppUserId",
                table: "EmployeeDocuments",
                newName: "IX_EmployeeDocuments_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeDocuments",
                table: "EmployeeDocuments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDocuments_AspNetUsers_AppUserId",
                table: "EmployeeDocuments",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocuments_AspNetUsers_AppUserId",
                table: "EmployeeDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeDocuments",
                table: "EmployeeDocuments");

            migrationBuilder.RenameTable(
                name: "EmployeeDocuments",
                newName: "EmployeeDocument");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeDocuments_AppUserId",
                table: "EmployeeDocument",
                newName: "IX_EmployeeDocument_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeDocument",
                table: "EmployeeDocument",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDocument_AspNetUsers_AppUserId",
                table: "EmployeeDocument",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
