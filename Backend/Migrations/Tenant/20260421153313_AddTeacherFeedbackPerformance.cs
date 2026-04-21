using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddTeacherFeedbackPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherFeedbackCycles",
                columns: table => new
                {
                    TeacherFeedbackCycleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OpensAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosesAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherFeedbackCycles", x => x.TeacherFeedbackCycleID);
                    table.ForeignKey(
                        name: "FK_TeacherFeedbackCycles_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherFeedbackCycles_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherFeedbackCycles_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackQuestions",
                columns: table => new
                {
                    FeedbackQuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherFeedbackCycleID = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    Audience = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackQuestions", x => x.FeedbackQuestionID);
                    table.ForeignKey(
                        name: "FK_FeedbackQuestions_TeacherFeedbackCycles_TeacherFeedbackCycleID",
                        column: x => x.TeacherFeedbackCycleID,
                        principalTable: "TeacherFeedbackCycles",
                        principalColumn: "TeacherFeedbackCycleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackSummaries",
                columns: table => new
                {
                    FeedbackSummaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherFeedbackCycleID = table.Column<int>(type: "int", nullable: false),
                    Audience = table.Column<int>(type: "int", nullable: false),
                    SubmittedCount = table.Column<int>(type: "int", nullable: false),
                    AverageNumericScore = table.Column<decimal>(type: "decimal(6,3)", nullable: true),
                    AggregateJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackSummaries", x => x.FeedbackSummaryID);
                    table.ForeignKey(
                        name: "FK_FeedbackSummaries_TeacherFeedbackCycles_TeacherFeedbackCycleID",
                        column: x => x.TeacherFeedbackCycleID,
                        principalTable: "TeacherFeedbackCycles",
                        principalColumn: "TeacherFeedbackCycleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentFeedbacks",
                columns: table => new
                {
                    ParentFeedbackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherFeedbackCycleID = table.Column<int>(type: "int", nullable: false),
                    GuardianID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentFeedbacks", x => x.ParentFeedbackID);
                    table.ForeignKey(
                        name: "FK_ParentFeedbacks_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentFeedbacks_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentFeedbacks_TeacherFeedbackCycles_TeacherFeedbackCycleID",
                        column: x => x.TeacherFeedbackCycleID,
                        principalTable: "TeacherFeedbackCycles",
                        principalColumn: "TeacherFeedbackCycleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentFeedbacks",
                columns: table => new
                {
                    StudentFeedbackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherFeedbackCycleID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFeedbacks", x => x.StudentFeedbackID);
                    table.ForeignKey(
                        name: "FK_StudentFeedbacks_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentFeedbacks_TeacherFeedbackCycles_TeacherFeedbackCycleID",
                        column: x => x.TeacherFeedbackCycleID,
                        principalTable: "TeacherFeedbackCycles",
                        principalColumn: "TeacherFeedbackCycleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackQuestions_TeacherFeedbackCycleID_SortOrder",
                table: "FeedbackQuestions",
                columns: new[] { "TeacherFeedbackCycleID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackSummaries_TeacherFeedbackCycleID_Audience",
                table: "FeedbackSummaries",
                columns: new[] { "TeacherFeedbackCycleID", "Audience" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentFeedbacks_GuardianID",
                table: "ParentFeedbacks",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_ParentFeedbacks_StudentID",
                table: "ParentFeedbacks",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_ParentFeedbacks_TeacherFeedbackCycleID_GuardianID_StudentID",
                table: "ParentFeedbacks",
                columns: new[] { "TeacherFeedbackCycleID", "GuardianID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeedbacks_StudentID",
                table: "StudentFeedbacks",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFeedbacks_TeacherFeedbackCycleID_StudentID",
                table: "StudentFeedbacks",
                columns: new[] { "TeacherFeedbackCycleID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherFeedbackCycles_AcademicYearID",
                table: "TeacherFeedbackCycles",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherFeedbackCycles_SchoolID_AcademicYearID_TeacherID_Status",
                table: "TeacherFeedbackCycles",
                columns: new[] { "SchoolID", "AcademicYearID", "TeacherID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherFeedbackCycles_TeacherID",
                table: "TeacherFeedbackCycles",
                column: "TeacherID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackQuestions");

            migrationBuilder.DropTable(
                name: "FeedbackSummaries");

            migrationBuilder.DropTable(
                name: "ParentFeedbacks");

            migrationBuilder.DropTable(
                name: "StudentFeedbacks");

            migrationBuilder.DropTable(
                name: "TeacherFeedbackCycles");
        }
    }
}
