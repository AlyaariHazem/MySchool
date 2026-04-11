using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddExamsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamSessions",
                columns: table => new
                {
                    ExamSessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSessions", x => x.ExamSessionID);
                    table.ForeignKey(
                        name: "FK_ExamSessions_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessions_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamTypes",
                columns: table => new
                {
                    ExamTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTypes", x => x.ExamTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledExams",
                columns: table => new
                {
                    ScheduledExamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamSessionID = table.Column<int>(type: "int", nullable: true),
                    ExamTypeID = table.Column<int>(type: "int", nullable: false),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    DivisionID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Room = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PassingMarks = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SchedulePublished = table.Column<bool>(type: "bit", nullable: false),
                    ResultsPublished = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledExams", x => x.ScheduledExamID);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_ExamSessions_ExamSessionID",
                        column: x => x.ExamSessionID,
                        principalTable: "ExamSessions",
                        principalColumn: "ExamSessionID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_ExamTypes_ExamTypeID",
                        column: x => x.ExamTypeID,
                        principalTable: "ExamTypes",
                        principalColumn: "ExamTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledExams_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamResults",
                columns: table => new
                {
                    ExamResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledExamID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsAbsent = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResults", x => x.ExamResultID);
                    table.ForeignKey(
                        name: "FK_ExamResults_ScheduledExams_ScheduledExamID",
                        column: x => x.ScheduledExamID,
                        principalTable: "ScheduledExams",
                        principalColumn: "ScheduledExamID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResults_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ExamTypes",
                columns: new[] { "ExamTypeID", "IsActive", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, true, "Midterm", 1 },
                    { 2, true, "Final", 2 },
                    { 3, true, "Quiz", 3 },
                    { 4, true, "Oral", 4 },
                    { 5, true, "Practical", 5 },
                    { 6, true, "Makeup", 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_ScheduledExamID_StudentID",
                table: "ExamResults",
                columns: new[] { "ScheduledExamID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamResults_StudentID",
                table: "ExamResults",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_TermID",
                table: "ExamSessions",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_YearID",
                table: "ExamSessions",
                column: "YearID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_ClassID",
                table: "ScheduledExams",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_DivisionID",
                table: "ScheduledExams",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_ExamDate",
                table: "ScheduledExams",
                column: "ExamDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_ExamSessionID",
                table: "ScheduledExams",
                column: "ExamSessionID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_ExamTypeID",
                table: "ScheduledExams",
                column: "ExamTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_SubjectID",
                table: "ScheduledExams",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_TeacherID",
                table: "ScheduledExams",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_TermID",
                table: "ScheduledExams",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledExams_YearID_TermID_ClassID_DivisionID",
                table: "ScheduledExams",
                columns: new[] { "YearID", "TermID", "ClassID", "DivisionID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamResults");

            migrationBuilder.DropTable(
                name: "ScheduledExams");

            migrationBuilder.DropTable(
                name: "ExamSessions");

            migrationBuilder.DropTable(
                name: "ExamTypes");
        }
    }
}
