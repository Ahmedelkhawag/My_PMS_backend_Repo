using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomFOAndHKStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HKStatus",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "BedType",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ViewType",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAdults",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Rooms",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Map existing RoomStatusId to HKStatus: Clean lookup -> 1, else 2
            migrationBuilder.Sql(@"
                UPDATE Rooms
                SET HKStatus = CASE
                    WHEN RoomStatusId IN (SELECT Id FROM RoomStatusLookups WHERE Name LIKE N'Clean%') THEN 1
                    ELSE 2
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HKStatus",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "BedType",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ViewType",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "MaxAdults",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Rooms");
        }
    }
}
