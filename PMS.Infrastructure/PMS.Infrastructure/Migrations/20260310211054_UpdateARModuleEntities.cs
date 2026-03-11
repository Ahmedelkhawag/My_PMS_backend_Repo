using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMS.Infrastructure.PMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateARModuleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditDays",
                table: "CompanyProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "CompanyProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCreditEnabled",
                table: "CompanyProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "ARPayments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ARPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ARAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllocatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ARAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ARAllocations_ARInvoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "ARInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ARAllocations_ARPayments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "ARPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ARPayments_InvoiceId",
                table: "ARPayments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ARAllocations_InvoiceId",
                table: "ARAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ARAllocations_PaymentId",
                table: "ARAllocations",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ARPayments_ARInvoices_InvoiceId",
                table: "ARPayments",
                column: "InvoiceId",
                principalTable: "ARInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ARPayments_ARInvoices_InvoiceId",
                table: "ARPayments");

            migrationBuilder.DropTable(
                name: "ARAllocations");

            migrationBuilder.DropIndex(
                name: "IX_ARPayments_InvoiceId",
                table: "ARPayments");

            migrationBuilder.DropColumn(
                name: "CreditDays",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "IsCreditEnabled",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "ARPayments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ARPayments");
        }
    }
}
