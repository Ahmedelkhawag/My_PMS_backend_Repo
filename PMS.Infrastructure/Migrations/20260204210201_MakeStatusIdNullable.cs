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
            // 1. الخطوة الأولى: نفتح الباب (نعدل العمود عشان يقبل NULL)
            migrationBuilder.AlterColumn<Guid>(
                name: "StatusID",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true, // هنا بنقوله اسمح بالـ NULL
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // 2. الخطوة الثانية: ننظف البيانات (دلوقتي عادي نحط NULL لأنه بقى مسموح)
            migrationBuilder.Sql("UPDATE AspNetUsers SET StatusID = NULL WHERE StatusID = '00000000-0000-0000-0000-000000000000'");

            // 3. الخطوة الثالثة: نربط العلاقة
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

            // 2. (مهم جداً) قبل ما نرجع العمود إجباري، لازم نملى أي خانة فاضية بقيمة
            // هنرجعها "أصفار" تاني زي ما كانت عشان التحويل ينجح
            migrationBuilder.Sql("UPDATE AspNetUsers SET StatusID = '00000000-0000-0000-0000-000000000000' WHERE StatusID IS NULL");

            // 3. نرجع العمود لنوعه القديم (غير قابل لـ NULL)
            migrationBuilder.AlterColumn<Guid>(
                name: "StatusID",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false, // رجعناه False يعني إجباري
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
