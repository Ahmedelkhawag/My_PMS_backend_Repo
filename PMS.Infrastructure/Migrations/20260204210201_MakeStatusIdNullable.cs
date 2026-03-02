using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeStatusIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Statuses_StatusID",
                table: "AspNetUsers");

            
            migrationBuilder.AlterColumn<Guid>(
                name: "StatusID",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true, 
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            
            migrationBuilder.Sql("UPDATE AspNetUsers SET StatusID = NULL WHERE StatusID = '00000000-0000-0000-0000-000000000000'");

            
            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Statuses_StatusID",
                table: "AspNetUsers",
                column: "StatusID",
                principalTable: "Statuses",
                principalColumn: "StatusID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
        name: "FK_AspNetUsers_Statuses_StatusID",
        table: "AspNetUsers");

            
            
            migrationBuilder.Sql("UPDATE AspNetUsers SET StatusID = '00000000-0000-0000-0000-000000000000' WHERE StatusID IS NULL");

            
            migrationBuilder.AlterColumn<Guid>(
                name: "StatusID",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false, 
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
