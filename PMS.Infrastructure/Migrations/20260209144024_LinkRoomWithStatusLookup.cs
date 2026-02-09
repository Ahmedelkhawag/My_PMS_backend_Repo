using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkRoomWithStatusLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarketSegment",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "MealPlan",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Rooms",
                newName: "RoomStatusId");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Reservations",
                newName: "MealPlanId");

            migrationBuilder.AddColumn<int>(
                name: "BookingSourceId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MarketSegmentId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "RoomStatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "RoomStatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "RoomStatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "RoomStatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "RoomStatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "RoomStatusId" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4 });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomStatusId",
                table: "Rooms",
                column: "RoomStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookingSourceId",
                table: "Reservations",
                column: "BookingSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_MarketSegmentId",
                table: "Reservations",
                column: "MarketSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_MealPlanId",
                table: "Reservations",
                column: "MealPlanId");

			migrationBuilder.Sql("UPDATE Rooms SET RoomStatusId = 1 WHERE RoomStatusId = 0 OR RoomStatusId IS NULL");

			migrationBuilder.AddForeignKey(
                name: "FK_Reservations_BookingSources_BookingSourceId",
                table: "Reservations",
                column: "BookingSourceId",
                principalTable: "BookingSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_MarketSegments_MarketSegmentId",
                table: "Reservations",
                column: "MarketSegmentId",
                principalTable: "MarketSegments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_MealPlans_MealPlanId",
                table: "Reservations",
                column: "MealPlanId",
                principalTable: "MealPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_RoomStatusLookups_RoomStatusId",
                table: "Rooms",
                column: "RoomStatusId",
                principalTable: "RoomStatusLookups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_BookingSources_BookingSourceId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_MarketSegments_MarketSegmentId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_MealPlans_MealPlanId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_RoomStatusLookups_RoomStatusId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_RoomStatusId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BookingSourceId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_MarketSegmentId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_MealPlanId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "BookingSourceId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "MarketSegmentId",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "RoomStatusId",
                table: "Rooms",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "MealPlanId",
                table: "Reservations",
                newName: "Source");

            migrationBuilder.AddColumn<string>(
                name: "MarketSegment",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MealPlan",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Status" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Status" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Status" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Status" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Status" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "Status" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 });
        }
    }
}
