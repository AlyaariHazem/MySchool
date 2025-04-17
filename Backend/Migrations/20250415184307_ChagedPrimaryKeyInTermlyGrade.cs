using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChagedPrimaryKeyInTermlyGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades");

            migrationBuilder.DropIndex(
                name: "IX_TermlyGrades_StudentID",
                table: "TermlyGrades");

            migrationBuilder.AddColumn<int>(
                name: "YearID",
                table: "TermlyGrades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades",
                column: "TermlyGradeID");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "ADMIN", "ADMIN" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "MANAGER", "MANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "STUDENT", "STUDENT" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "TEACHER", "TEACHER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "5", null, "GUARDIAN", "GUARDIAN" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "Email", "HireDate", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName", "UserType" },
                values: new object[] { "658000ad-682a-4611-9071-b5123adfcefc", "ADMIN@GMAIL.COM", new DateTime(2025, 4, 15, 21, 43, 5, 137, DateTimeKind.Local).AddTicks(5713), "ADMIN@GMAIL.COM", "ADMIN", "AQAAAAIAAYagAAAAEBzf3iHsDiNTk8adjuEGxpJLnoGtJJgrZlUEQ7W1lwF4KFWsjzKVzITUaE1caEQzJw==", "12570dc5-4457-4b1a-ae73-f7f3fe9fd8eb", "ADMIN", "ADMIN" });

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_StudentID_TermID_SubjectID_ClassID",
                table: "TermlyGrades",
                columns: new[] { "StudentID", "TermID", "SubjectID", "ClassID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_TermID",
                table: "TermlyGrades",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_YearID",
                table: "TermlyGrades",
                column: "YearID");

            migrationBuilder.AddForeignKey(
                name: "FK_TermlyGrades_Years_YearID",
                table: "TermlyGrades",
                column: "YearID",
                principalTable: "Years",
                principalColumn: "YearID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermlyGrades_Years_YearID",
                table: "TermlyGrades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades");

            migrationBuilder.DropIndex(
                name: "IX_TermlyGrades_StudentID_TermID_SubjectID_ClassID",
                table: "TermlyGrades");

            migrationBuilder.DropIndex(
                name: "IX_TermlyGrades_TermID",
                table: "TermlyGrades");

            migrationBuilder.DropIndex(
                name: "IX_TermlyGrades_YearID",
                table: "TermlyGrades");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5");

            migrationBuilder.DropColumn(
                name: "YearID",
                table: "TermlyGrades");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TermlyGrades",
                table: "TermlyGrades",
                column: "TermID");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "MANAGER", "MANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "STUDENT", "STUDENT" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "TEACHER", "TEACHER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "GUARDIAN", "GUARDIAN" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "Email", "HireDate", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName", "UserType" },
                values: new object[] { "e80c72af-78ab-43df-a1e3-81f5598b4d9c", "ALYAARIHAZEM@GMAIL.COM", new DateTime(2025, 4, 12, 23, 7, 18, 293, DateTimeKind.Local).AddTicks(7539), "ALYAARIHAZEM@GMAIL.COM", "MANAGER", "AQAAAAIAAYagAAAAEDkPEvEcdvTt84yukm1NmriLfFXr/wu7R3V8viJ/FHZRdCL385oUgmMbZaOKEm9aDw==", "f06faea1-4be6-4964-99ed-402719426ed7", "MANAGER", "MANAGER" });

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_StudentID",
                table: "TermlyGrades",
                column: "StudentID");
        }
    }
}
