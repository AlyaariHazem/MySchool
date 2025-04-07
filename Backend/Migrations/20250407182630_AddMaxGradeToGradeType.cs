using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxGradeToGradeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName_MiddleName",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName_MiddleName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName_MiddleName",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GradeTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxGrade",
                table: "GradeTypes",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3062d71a-0f68-41e2-bfcc-4fadaabe5bf4", new DateTime(2025, 4, 7, 21, 26, 27, 976, DateTimeKind.Local).AddTicks(6393), "AQAAAAIAAYagAAAAEAuj+Ln0RWPjGXrtI7j8PHyLV3W4/+4/CTWCBnQFZotn56TJiechxcO8v1soJCTwpQ==", "e34dd158-a54c-4ada-9c7b-f0e2fc01503c" });

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 1,
                columns: new[] { "IsActive", "MaxGrade" },
                values: new object[] { true, 20 });

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 2,
                columns: new[] { "IsActive", "MaxGrade" },
                values: new object[] { true, 20 });

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 3,
                columns: new[] { "IsActive", "MaxGrade" },
                values: new object[] { true, 10 });

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 4,
                columns: new[] { "IsActive", "MaxGrade" },
                values: new object[] { true, 10 });

            migrationBuilder.UpdateData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 5,
                columns: new[] { "IsActive", "MaxGrade" },
                values: new object[] { true, 40 });

            migrationBuilder.InsertData(
                table: "GradeTypes",
                columns: new[] { "GradeTypeID", "IsActive", "MaxGrade", "Name" },
                values: new object[,]
                {
                    { 6, false, 20, "work" },
                    { 7, false, 30, "lab" },
                    { 8, false, 20, "skills" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "GradeTypes",
                keyColumn: "GradeTypeID",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GradeTypes");

            migrationBuilder.DropColumn(
                name: "MaxGrade",
                table: "GradeTypes");

            migrationBuilder.AlterColumn<string>(
                name: "FullName_MiddleName",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName_MiddleName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName_MiddleName",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b938daf4-68f6-4ea6-be21-ebf3e47c5de4", new DateTime(2025, 4, 7, 16, 25, 9, 133, DateTimeKind.Local).AddTicks(1249), "AQAAAAIAAYagAAAAEH9NyqoKJUXbp5HuOe+gQe8bYF0wzRGDzgu31ObF0t24xifbv9d/a789CNIqTipkdA==", "31c22093-fe60-4d77-8aae-17da72a55b68" });
        }
    }
}
