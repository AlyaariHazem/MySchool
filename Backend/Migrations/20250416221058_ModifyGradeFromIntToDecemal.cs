using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ModifyGradeFromIntToDecemal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MaxGrade",
                table: "GradeTypes",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4df2c51c-72f8-445e-a4bb-4d0615f57e2f", new DateTime(2025, 4, 17, 1, 10, 57, 163, DateTimeKind.Local).AddTicks(7824), "AQAAAAIAAYagAAAAEFYtYhGwNeuyTCNCu+V+D91AJ/9jbpLSDzRvRtppdk7jQCKkhq1tR/XbYxKVR+lW9g==", "7a8ed15d-163c-46a2-af7a-8738d98dcf37" });

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 1,
                column: "MaxGrade",
                value: 20m);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 2,
                column: "MaxGrade",
                value: 20m);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 3,
                column: "MaxGrade",
                value: 10m);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 4,
                column: "MaxGrade",
                value: 10m);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 5,
                column: "MaxGrade",
                value: 40m);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 6,
                column: "MaxGrade",
                value: 20m);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 7,
                column: "MaxGrade",
                value: 30m);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 8,
                column: "MaxGrade",
                value: 20m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MaxGrade",
                table: "GradeTypes",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7b10ebc5-2488-44e7-9a2c-2e386e439363", new DateTime(2025, 4, 16, 15, 37, 28, 866, DateTimeKind.Local).AddTicks(1626), "AQAAAAIAAYagAAAAEMQ4XxyeR9JGxZh+6fHyny1h1mygzMKMMab7dyntsR9N+r+uYM7pePY9ggvNYLeggA==", "6851637c-f9c8-4c1b-8cb5-7faf8414ebcb" });

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 1,
                column: "MaxGrade",
                value: 20);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 2,
                column: "MaxGrade",
                value: 20);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 3,
                column: "MaxGrade",
                value: 10);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 4,
                column: "MaxGrade",
                value: 10);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 5,
                column: "MaxGrade",
                value: 40);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 6,
                column: "MaxGrade",
                value: 20);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 7,
                column: "MaxGrade",
                value: 30);

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 8,
                column: "MaxGrade",
                value: 20);
        }
    }
}
