using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class MadeTermlyGradeIdIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. إزالة المفتاح الأساسي
            migrationBuilder.DropPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades");

            // 2. حذف العمود القديم
            migrationBuilder.DropColumn(
                name: "TermlyGradeID",
                table: "TermlyGrades");

            // 3. إعادة إنشاء العمود مع Identity
            migrationBuilder.AddColumn<int>(
                name: "TermlyGradeID",
                table: "TermlyGrades",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 4. إعادة تعيين المفتاح الأساسي
            migrationBuilder.AddPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades",
                column: "TermlyGradeID");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7b10ebc5-2488-44e7-9a2c-2e386e439363", new DateTime(2025, 4, 16, 15, 37, 28, 866, DateTimeKind.Local).AddTicks(1626), "AQAAAAIAAYagAAAAEMQ4XxyeR9JGxZh+6fHyny1h1mygzMKMMab7dyntsR9N+r+uYM7pePY9ggvNYLeggA==", "6851637c-f9c8-4c1b-8cb5-7faf8414ebcb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. إزالة المفتاح الأساسي
            migrationBuilder.DropPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades");

            // 2. حذف العمود الجديد
            migrationBuilder.DropColumn(
                name: "TermlyGradeID",
                table: "TermlyGrades");

            // 3. إعادة العمود بدون خاصية Identity
            migrationBuilder.AddColumn<int>(
                name: "TermlyGradeID",
                table: "TermlyGrades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 4. إعادة تعيين المفتاح الأساسي
            migrationBuilder.AddPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades",
                column: "TermlyGradeID");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "923d87ae-4ad0-4bc7-ba21-cdba47dbc9c7", new DateTime(2025, 4, 15, 23, 30, 56, 613, DateTimeKind.Local).AddTicks(1854), "AQAAAAIAAYagAAAAEH092gzxaA6EXhSvxR8nSVswHeYNxQV0cY9uXWrBttxn5lNxkZIvAODSTFVw7/OIDA==", "eaa23200-1614-4613-bbf9-33fa8d691576" });
        }
    }
}
