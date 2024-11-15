using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class chanageNameAllowNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName_SecondName",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "FullName_SecondName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FullName_SecondName",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "FullName_SecondName",
                table: "Guardians");

            migrationBuilder.RenameColumn(
                name: "FullName_ThirdName",
                table: "Teachers",
                newName: "FullName_MiddleName");

            migrationBuilder.RenameColumn(
                name: "FullName_ThirdName",
                table: "Students",
                newName: "FullName_MiddleName");

            migrationBuilder.RenameColumn(
                name: "FullName_ThirdName",
                table: "Managers",
                newName: "FullName_MiddleName");

            migrationBuilder.RenameColumn(
                name: "FullName_ThirdName",
                table: "Guardians",
                newName: "FullName_MiddleName");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "YearDateEnd",
                table: "Years",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "zone",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName_MiddleName",
                table: "Teachers",
                newName: "FullName_ThirdName");

            migrationBuilder.RenameColumn(
                name: "FullName_MiddleName",
                table: "Students",
                newName: "FullName_ThirdName");

            migrationBuilder.RenameColumn(
                name: "FullName_MiddleName",
                table: "Managers",
                newName: "FullName_ThirdName");

            migrationBuilder.RenameColumn(
                name: "FullName_MiddleName",
                table: "Guardians",
                newName: "FullName_ThirdName");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "YearDateEnd",
                table: "Years",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName_SecondName",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName_SecondName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "zone",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName_SecondName",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName_SecondName",
                table: "Guardians",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
