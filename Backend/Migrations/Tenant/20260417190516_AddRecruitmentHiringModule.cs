using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddRecruitmentHiringModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    JobPostingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    EmployeeJobTypeID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Responsibilities = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    NumberOfOpenings = table.Column<int>(type: "int", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.JobPostingID);
                    table.ForeignKey(
                        name: "FK_JobPostings_EmployeeJobTypes_EmployeeJobTypeID",
                        column: x => x.EmployeeJobTypeID,
                        principalTable: "EmployeeJobTypes",
                        principalColumn: "EmployeeJobTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPostings_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobPostings_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    JobApplicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobPostingID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    ApplicantFirstName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ApplicantLastName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ApplicantArabicName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ApplicantEnglishName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NationalID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    HighestQualification = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: true),
                    CurrentEmployer = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResumeFileUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CoverLetter = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ConvertedEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.JobApplicationID);
                    table.ForeignKey(
                        name: "FK_JobApplications_EmployeeProfiles_ConvertedEmployeeProfileID",
                        column: x => x.ConvertedEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_JobPostings_JobPostingID",
                        column: x => x.JobPostingID,
                        principalTable: "JobPostings",
                        principalColumn: "JobPostingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HiringDecisions",
                columns: table => new
                {
                    HiringDecisionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    DecisionStatus = table.Column<int>(type: "int", nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedByUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecidedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    OfferJobTypeID = table.Column<int>(type: "int", nullable: false),
                    ProposedHireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProposedSalaryNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ConvertedEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringDecisions", x => x.HiringDecisionID);
                    table.ForeignKey(
                        name: "FK_HiringDecisions_EmployeeJobTypes_OfferJobTypeID",
                        column: x => x.OfferJobTypeID,
                        principalTable: "EmployeeJobTypes",
                        principalColumn: "EmployeeJobTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringDecisions_EmployeeProfiles_ConvertedEmployeeProfileID",
                        column: x => x.ConvertedEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringDecisions_EmployeeProfiles_DecidedByEmployeeProfileID",
                        column: x => x.DecidedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringDecisions_JobApplications_JobApplicationID",
                        column: x => x.JobApplicationID,
                        principalTable: "JobApplications",
                        principalColumn: "JobApplicationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringDecisions_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HiringDecisions_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecruitmentInterviews",
                columns: table => new
                {
                    InterviewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    InterviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InterviewType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LocationOrMeetingLink = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    InterviewerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    InterviewerUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    InterviewerEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentInterviews", x => x.InterviewID);
                    table.ForeignKey(
                        name: "FK_RecruitmentInterviews_EmployeeProfiles_InterviewerEmployeeProfileID",
                        column: x => x.InterviewerEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentInterviews_JobApplications_JobApplicationID",
                        column: x => x.JobApplicationID,
                        principalTable: "JobApplications",
                        principalColumn: "JobApplicationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentInterviews_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentInterviews_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CandidateEvaluations",
                columns: table => new
                {
                    CandidateEvaluationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationID = table.Column<int>(type: "int", nullable: false),
                    InterviewID = table.Column<int>(type: "int", nullable: true),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    EvaluatorUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EvaluatorEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    TechnicalScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CommunicationScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClassManagementScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CultureFitScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OverallScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Strengths = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Weaknesses = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Recommendation = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateEvaluations", x => x.CandidateEvaluationID);
                    table.ForeignKey(
                        name: "FK_CandidateEvaluations_EmployeeProfiles_EvaluatorEmployeeProfileID",
                        column: x => x.EvaluatorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateEvaluations_JobApplications_JobApplicationID",
                        column: x => x.JobApplicationID,
                        principalTable: "JobApplications",
                        principalColumn: "JobApplicationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateEvaluations_RecruitmentInterviews_InterviewID",
                        column: x => x.InterviewID,
                        principalTable: "RecruitmentInterviews",
                        principalColumn: "InterviewID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CandidateEvaluations_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidateEvaluations_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_AcademicYearID",
                table: "CandidateEvaluations",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_EvaluatorEmployeeProfileID",
                table: "CandidateEvaluations",
                column: "EvaluatorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_InterviewID",
                table: "CandidateEvaluations",
                column: "InterviewID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_JobApplicationID",
                table: "CandidateEvaluations",
                column: "JobApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_SchoolID",
                table: "CandidateEvaluations",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_AcademicYearID",
                table: "HiringDecisions",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_ConvertedEmployeeProfileID",
                table: "HiringDecisions",
                column: "ConvertedEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_DecidedByEmployeeProfileID",
                table: "HiringDecisions",
                column: "DecidedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_DecisionStatus",
                table: "HiringDecisions",
                column: "DecisionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_JobApplicationID",
                table: "HiringDecisions",
                column: "JobApplicationID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_OfferJobTypeID",
                table: "HiringDecisions",
                column: "OfferJobTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_HiringDecisions_SchoolID",
                table: "HiringDecisions",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_AcademicYearID",
                table: "JobApplications",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_ConvertedEmployeeProfileID",
                table: "JobApplications",
                column: "ConvertedEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Email",
                table: "JobApplications",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobPostingID",
                table: "JobApplications",
                column: "JobPostingID");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_NationalID",
                table: "JobApplications",
                column: "NationalID");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_SchoolID",
                table: "JobApplications",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_Status",
                table: "JobApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_AcademicYearID",
                table: "JobPostings",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_EmployeeJobTypeID",
                table: "JobPostings",
                column: "EmployeeJobTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_SchoolID",
                table: "JobPostings",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_Status",
                table: "JobPostings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentInterviews_AcademicYearID",
                table: "RecruitmentInterviews",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentInterviews_InterviewDate",
                table: "RecruitmentInterviews",
                column: "InterviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentInterviews_InterviewerEmployeeProfileID",
                table: "RecruitmentInterviews",
                column: "InterviewerEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentInterviews_JobApplicationID",
                table: "RecruitmentInterviews",
                column: "JobApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentInterviews_SchoolID",
                table: "RecruitmentInterviews",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentInterviews_Status",
                table: "RecruitmentInterviews",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateEvaluations");

            migrationBuilder.DropTable(
                name: "HiringDecisions");

            migrationBuilder.DropTable(
                name: "RecruitmentInterviews");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobPostings");
        }
    }
}
