using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddHomeworkModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeworkTasks",
                columns: table => new
                {
                    HomeworkTaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    DivisionID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkTasks", x => x.HomeworkTaskID);
                    table.ForeignKey(
                        name: "FK_HomeworkTasks_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkTasks_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkTasks_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkTasks_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkTasks_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HomeworkTasks_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkTaskLinks",
                columns: table => new
                {
                    HomeworkTaskLinkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeworkTaskID = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkTaskLinks", x => x.HomeworkTaskLinkID);
                    table.ForeignKey(
                        name: "FK_HomeworkTaskLinks_HomeworkTasks_HomeworkTaskID",
                        column: x => x.HomeworkTaskID,
                        principalTable: "HomeworkTasks",
                        principalColumn: "HomeworkTaskID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkSubmissions",
                columns: table => new
                {
                    HomeworkSubmissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeworkTaskID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeacherFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FeedbackPublished = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkSubmissions", x => x.HomeworkSubmissionID);
                    table.ForeignKey(
                        name: "FK_HomeworkSubmissions_HomeworkTasks_HomeworkTaskID",
                        column: x => x.HomeworkTaskID,
                        principalTable: "HomeworkTasks",
                        principalColumn: "HomeworkTaskID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeworkSubmissions_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HomeworkSubmissionFiles",
                columns: table => new
                {
                    HomeworkSubmissionFileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeworkSubmissionID = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkSubmissionFiles", x => x.HomeworkSubmissionFileID);
                    table.ForeignKey(
                        name: "FK_HomeworkSubmissionFiles_HomeworkSubmissions_HomeworkSubmissionID",
                        column: x => x.HomeworkSubmissionID,
                        principalTable: "HomeworkSubmissions",
                        principalColumn: "HomeworkSubmissionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkSubmissionFiles_HomeworkSubmissionID",
                table: "HomeworkSubmissionFiles",
                column: "HomeworkSubmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkSubmissions_HomeworkTaskID_StudentID",
                table: "HomeworkSubmissions",
                columns: new[] { "HomeworkTaskID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkSubmissions_StudentID",
                table: "HomeworkSubmissions",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTaskLinks_HomeworkTaskID",
                table: "HomeworkTaskLinks",
                column: "HomeworkTaskID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_ClassID",
                table: "HomeworkTasks",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_DivisionID",
                table: "HomeworkTasks",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_DueDateUtc",
                table: "HomeworkTasks",
                column: "DueDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_SubjectID",
                table: "HomeworkTasks",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_TeacherID",
                table: "HomeworkTasks",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_TermID",
                table: "HomeworkTasks",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_YearID_TermID_ClassID_DivisionID",
                table: "HomeworkTasks",
                columns: new[] { "YearID", "TermID", "ClassID", "DivisionID" });

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_YearID",
                table: "HomeworkTasks",
                column: "YearID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HomeworkSubmissionFiles");
            migrationBuilder.DropTable(name: "HomeworkTaskLinks");
            migrationBuilder.DropTable(name: "HomeworkSubmissions");
            migrationBuilder.DropTable(name: "HomeworkTasks");
        }
    }
}
