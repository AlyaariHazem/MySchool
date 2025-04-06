using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedMonthTermGradeCoursPlane : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GradeTypes",
                columns: table => new
                {
                    GradeTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeTypes", x => x.GradeTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    TermID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YearID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.TermID);
                    table.ForeignKey(
                        name: "FK_Terms_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoursePlans",
                columns: table => new
                {
                    CoursePlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    DivisionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePlans", x => x.CoursePlanID);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Months",
                columns: table => new
                {
                    MonthID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Months", x => x.MonthID);
                    table.ForeignKey(
                        name: "FK_Months_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyGrades",
                columns: table => new
                {
                    MonthlyGradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    MonthID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    GradeTypeID = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyGrades", x => x.MonthlyGradeID);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_GradeTypes_GradeTypeID",
                        column: x => x.GradeTypeID,
                        principalTable: "GradeTypes",
                        principalColumn: "GradeTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Months_MonthID",
                        column: x => x.MonthID,
                        principalTable: "Months",
                        principalColumn: "MonthID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID");
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ea234470-4dc7-42f0-9003-e2177fab6129", new DateTime(2025, 4, 5, 23, 0, 1, 789, DateTimeKind.Local).AddTicks(3951), "AQAAAAIAAYagAAAAECKT88mV4k1rFR45rIvJG3IJsEUXyqemHHAkBsvqchvFy0L2H8BgizPqjnIl96+QOw==", "d9bbcb4d-2b81-422f-bc20-ab140ec3cf9f" });

            migrationBuilder.InsertData(
                table: "GradeTypes",
                columns: new[] { "GradeTypeID", "Name" },
                values: new object[,]
                {
                    { 1, "Assignments" },
                    { 2, "Attendance" },
                    { 3, "Participation" },
                    { 4, "Oral" },
                    { 5, "Exam" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_ClassID",
                table: "CoursePlans",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_DivisionID",
                table: "CoursePlans",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_SubjectID",
                table: "CoursePlans",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_TeacherID",
                table: "CoursePlans",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_TermID",
                table: "CoursePlans",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_YearID",
                table: "CoursePlans",
                column: "YearID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_ClassID",
                table: "MonthlyGrades",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_GradeTypeID",
                table: "MonthlyGrades",
                column: "GradeTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_MonthID",
                table: "MonthlyGrades",
                column: "MonthID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_StudentID_SubjectID_MonthID_GradeTypeID",
                table: "MonthlyGrades",
                columns: new[] { "StudentID", "SubjectID", "MonthID", "GradeTypeID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_SubjectID",
                table: "MonthlyGrades",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_TermID",
                table: "MonthlyGrades",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_Months_TermID",
                table: "Months",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_YearID",
                table: "Terms",
                column: "YearID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoursePlans");

            migrationBuilder.DropTable(
                name: "MonthlyGrades");

            migrationBuilder.DropTable(
                name: "GradeTypes");

            migrationBuilder.DropTable(
                name: "Months");

            migrationBuilder.DropTable(
                name: "Terms");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "683c84d0-24e6-48af-81dd-8892fd24a1d4", new DateTime(2025, 2, 27, 3, 49, 30, 203, DateTimeKind.Local).AddTicks(5919), "AQAAAAIAAYagAAAAEDzYYajqNuv+djJbS7f8NUKKgbhXCeQe0md5BvhouU0V2/fxituNusXPDCdG1moTzA==", "40fd6df1-ef73-45ca-927c-65095b5a410e" });
        }
    }
}
