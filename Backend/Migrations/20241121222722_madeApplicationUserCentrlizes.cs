using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class madeApplicationUserCentrlizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students");

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PhoneNum",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PlaceBirth",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Students",
                newName: "FullNameEng_MiddleNameEng");

            migrationBuilder.RenameColumn(
                name: "TypeGuardian",
                table: "Guardians",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Teachers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullNameEng_FirstNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullNameEng_LastNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Managers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Guardians",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PlaceBirth",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserID",
                table: "Teachers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserID",
                table: "Students",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_UserID",
                table: "Managers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_UserID",
                table: "Guardians",
                column: "UserID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Guardians_AspNetUsers_UserID",
                table: "Guardians",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_AspNetUsers_UserID",
                table: "Managers",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_UserID",
                table: "Students",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students",
                column: "GuardianID",
                principalTable: "Guardians",
                principalColumn: "GuardianID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_UserID",
                table: "Teachers",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guardians_AspNetUsers_UserID",
                table: "Guardians");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_AspNetUsers_UserID",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_UserID",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_UserID",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_UserID",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_UserID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Managers_UserID",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Guardians_UserID",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "FullNameEng_FirstNameEng",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FullNameEng_LastNameEng",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PlaceBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "FullNameEng_MiddleNameEng",
                table: "Students",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Guardians",
                newName: "TypeGuardian");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Teachers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "HireDate",
                table: "Teachers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "PhoneNum",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Students",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceBirth",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Managers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Guardians",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Guardians",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Guardians",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Guardians",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Phone",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Guardians",
                columns: new[] { "GuardianID", "Address", "DateOfBirth", "Email", "FullName", "Phone", "TypeGuardian" },
                values: new object[,]
                {
                    { 1, null, null, null, "School", null, null },
                    { 2, null, null, null, "Branches", null, null },
                    { 3, null, null, null, "Fuands", null, null },
                    { 4, null, null, null, "Guardians", null, null },
                    { 5, null, null, null, "Employees", null, null },
                    { 6, null, null, null, "Bacnks", null, null }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students",
                column: "GuardianID",
                principalTable: "Guardians",
                principalColumn: "GuardianID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
