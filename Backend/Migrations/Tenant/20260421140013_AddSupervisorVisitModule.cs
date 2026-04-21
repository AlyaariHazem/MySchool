using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddSupervisorVisitModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupervisorVisits",
                columns: table => new
                {
                    SupervisorVisitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    VisitedTeacherID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: true),
                    SubjectID = table.Column<int>(type: "int", nullable: true),
                    SupervisorEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    VisitDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OverallScoreOutOf100 = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SummaryNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorVisits", x => x.SupervisorVisitID);
                    table.ForeignKey(
                        name: "FK_SupervisorVisits_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorVisits_EmployeeProfiles_SupervisorEmployeeProfileID",
                        column: x => x.SupervisorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorVisits_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorVisits_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorVisits_Teachers_VisitedTeacherID",
                        column: x => x.VisitedTeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorVisits_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitObservations",
                columns: table => new
                {
                    VisitObservationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupervisorVisitID = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ObservationText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitObservations", x => x.VisitObservationID);
                    table.ForeignKey(
                        name: "FK_VisitObservations_SupervisorVisits_SupervisorVisitID",
                        column: x => x.SupervisorVisitID,
                        principalTable: "SupervisorVisits",
                        principalColumn: "SupervisorVisitID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitRecommendations",
                columns: table => new
                {
                    VisitRecommendationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupervisorVisitID = table.Column<int>(type: "int", nullable: false),
                    RecommendationText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    ImplementationStatus = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitRecommendations", x => x.VisitRecommendationID);
                    table.ForeignKey(
                        name: "FK_VisitRecommendations_SupervisorVisits_SupervisorVisitID",
                        column: x => x.SupervisorVisitID,
                        principalTable: "SupervisorVisits",
                        principalColumn: "SupervisorVisitID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationFollowUps",
                columns: table => new
                {
                    RecommendationFollowUpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitRecommendationID = table.Column<int>(type: "int", nullable: false),
                    FollowUpNote = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    FollowUpDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FollowUpByEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationFollowUps", x => x.RecommendationFollowUpID);
                    table.ForeignKey(
                        name: "FK_RecommendationFollowUps_EmployeeProfiles_FollowUpByEmployeeProfileID",
                        column: x => x.FollowUpByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecommendationFollowUps_VisitRecommendations_VisitRecommendationID",
                        column: x => x.VisitRecommendationID,
                        principalTable: "VisitRecommendations",
                        principalColumn: "VisitRecommendationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationFollowUps_FollowUpByEmployeeProfileID",
                table: "RecommendationFollowUps",
                column: "FollowUpByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationFollowUps_VisitRecommendationID_FollowUpDate_CreatedAtUtc",
                table: "RecommendationFollowUps",
                columns: new[] { "VisitRecommendationID", "FollowUpDate", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorVisits_AcademicYearID",
                table: "SupervisorVisits",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorVisits_ClassID",
                table: "SupervisorVisits",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorVisits_SchoolID_AcademicYearID_VisitedTeacherID_VisitDate",
                table: "SupervisorVisits",
                columns: new[] { "SchoolID", "AcademicYearID", "VisitedTeacherID", "VisitDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorVisits_SubjectID",
                table: "SupervisorVisits",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorVisits_SupervisorEmployeeProfileID",
                table: "SupervisorVisits",
                column: "SupervisorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorVisits_VisitedTeacherID",
                table: "SupervisorVisits",
                column: "VisitedTeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_VisitObservations_SupervisorVisitID_SortOrder",
                table: "VisitObservations",
                columns: new[] { "SupervisorVisitID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_VisitRecommendations_SupervisorVisitID_SortOrder",
                table: "VisitRecommendations",
                columns: new[] { "SupervisorVisitID", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecommendationFollowUps");

            migrationBuilder.DropTable(
                name: "VisitObservations");

            migrationBuilder.DropTable(
                name: "VisitRecommendations");

            migrationBuilder.DropTable(
                name: "SupervisorVisits");
        }
    }
}
