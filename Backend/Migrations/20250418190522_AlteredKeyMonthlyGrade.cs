using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AlteredKeyMonthlyGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlyGrades",
                table: "MonthlyGrades");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlyGrades",
                table: "MonthlyGrades",
                columns: new[] { "StudentID", "YearID", "SubjectID", "MonthID", "GradeTypeID", "ClassID", "TermID" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "da925df1-2f04-4c91-a39e-d8c15879a490", new DateTime(2025, 4, 18, 22, 5, 20, 363, DateTimeKind.Local).AddTicks(1516), "AQAAAAIAAYagAAAAEHUfqMWqU5V/GQ9c0QHPVwfiz82avdTMOxcUdcBX37+GzAeUe5ZSbBAUXnox8tWMRQ==", "ed725e68-f34c-417e-9cfd-5a3c2131df45" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlyGrades",
                table: "MonthlyGrades");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlyGrades",
                table: "MonthlyGrades",
                columns: new[] { "StudentID", "YearID", "SubjectID", "MonthID", "GradeTypeID", "ClassID" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4df2c51c-72f8-445e-a4bb-4d0615f57e2f", new DateTime(2025, 4, 17, 1, 10, 57, 163, DateTimeKind.Local).AddTicks(7824), "AQAAAAIAAYagAAAAEFYtYhGwNeuyTCNCu+V+D91AJ/9jbpLSDzRvRtppdk7jQCKkhq1tR/XbYxKVR+lW9g==", "7a8ed15d-163c-46a2-af7a-8738d98dcf37" });
        }
    }
}
