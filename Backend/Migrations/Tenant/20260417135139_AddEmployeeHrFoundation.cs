using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddEmployeeHrFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeJobTypes",
                columns: table => new
                {
                    EmployeeJobTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeJobTypes", x => x.EmployeeJobTypeID);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeProfiles",
                columns: table => new
                {
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    CurrentAcademicYearID = table.Column<int>(type: "int", nullable: false),
                    EmployeeJobTypeID = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FullName_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullNameAlis_FirstNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameAlis_MiddleNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameAlis_LastNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmploymentStatus = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: true),
                    ManagerID = table.Column<int>(type: "int", nullable: true),
                    SchoolStaffID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProfiles", x => x.EmployeeProfileID);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_EmployeeJobTypes_EmployeeJobTypeID",
                        column: x => x.EmployeeJobTypeID,
                        principalTable: "EmployeeJobTypes",
                        principalColumn: "EmployeeJobTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_Managers_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "Managers",
                        principalColumn: "ManagerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_SchoolStaff_SchoolStaffID",
                        column: x => x.SchoolStaffID,
                        principalTable: "SchoolStaff",
                        principalColumn: "SchoolStaffID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_Years_CurrentAcademicYearID",
                        column: x => x.CurrentAcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDocuments",
                columns: table => new
                {
                    EmployeeDocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDocuments", x => x.EmployeeDocumentID);
                    table.ForeignKey(
                        name: "FK_EmployeeDocuments_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeHistories",
                columns: table => new
                {
                    EmployeeHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    EmployeeJobTypeID = table.Column<int>(type: "int", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeHistories", x => x.EmployeeHistoryID);
                    table.ForeignKey(
                        name: "FK_EmployeeHistories_EmployeeJobTypes_EmployeeJobTypeID",
                        column: x => x.EmployeeJobTypeID,
                        principalTable: "EmployeeJobTypes",
                        principalColumn: "EmployeeJobTypeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmployeeHistories_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeHistories_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeHistories_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaves",
                columns: table => new
                {
                    EmployeeLeaveID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaves", x => x.EmployeeLeaveID);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaves_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaves_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePerformanceSummaries",
                columns: table => new
                {
                    EmployeePerformanceSummaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    EmployeeJobTypeID = table.Column<int>(type: "int", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EvaluationScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AchievementPoints = table.Column<int>(type: "int", nullable: false),
                    ViolationPoints = table.Column<int>(type: "int", nullable: false),
                    RequestCount = table.Column<int>(type: "int", nullable: false),
                    ActivityCount = table.Column<int>(type: "int", nullable: false),
                    PerformanceLevel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    StrengthsSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    WeaknessesSummary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FinalNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePerformanceSummaries", x => x.EmployeePerformanceSummaryID);
                    table.ForeignKey(
                        name: "FK_EmployeePerformanceSummaries_EmployeeJobTypes_EmployeeJobTypeID",
                        column: x => x.EmployeeJobTypeID,
                        principalTable: "EmployeeJobTypes",
                        principalColumn: "EmployeeJobTypeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmployeePerformanceSummaries_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePerformanceSummaries_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeePerformanceSummaries_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeQualifications",
                columns: table => new
                {
                    EmployeeQualificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    DegreeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Major = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Institution = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    GraduationYear = table.Column<int>(type: "int", nullable: true),
                    GradeOrScore = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeQualifications", x => x.EmployeeQualificationID);
                    table.ForeignKey(
                        name: "FK_EmployeeQualifications_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSpecializations",
                columns: table => new
                {
                    EmployeeSpecializationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Level = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSpecializations", x => x.EmployeeSpecializationID);
                    table.ForeignKey(
                        name: "FK_EmployeeSpecializations_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EmployeeJobTypes",
                columns: new[] { "EmployeeJobTypeID", "Code", "IsActive", "Name", "NameAr", "SortOrder" },
                values: new object[,]
                {
                    { 1, "TEACHER", true, "Teacher", "معلم", 1 },
                    { 2, "MANAGER", true, "Manager", "مدير", 2 },
                    { 3, "SCHOOL_STAFF", true, "School staff", "موظف مدرسة", 3 },
                    { 4, "ADMINISTRATOR", true, "Administrator", "إداري", 4 },
                    { 5, "SUPPORT", true, "Support", "دعم", 5 },
                    { 6, "OTHER", true, "Other", "أخرى", 99 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_EmployeeProfileID",
                table: "EmployeeDocuments",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHistories_AcademicYearID",
                table: "EmployeeHistories",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHistories_EmployeeJobTypeID",
                table: "EmployeeHistories",
                column: "EmployeeJobTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHistories_EmployeeProfileID_AcademicYearID",
                table: "EmployeeHistories",
                columns: new[] { "EmployeeProfileID", "AcademicYearID" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHistories_SchoolID",
                table: "EmployeeHistories",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeJobTypes_Code",
                table: "EmployeeJobTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaves_AcademicYearID",
                table: "EmployeeLeaves",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaves_EmployeeProfileID_AcademicYearID",
                table: "EmployeeLeaves",
                columns: new[] { "EmployeeProfileID", "AcademicYearID" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePerformanceSummaries_AcademicYearID",
                table: "EmployeePerformanceSummaries",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePerformanceSummaries_EmployeeJobTypeID",
                table: "EmployeePerformanceSummaries",
                column: "EmployeeJobTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePerformanceSummaries_EmployeeProfileID_AcademicYearID_GeneratedAtUtc",
                table: "EmployeePerformanceSummaries",
                columns: new[] { "EmployeeProfileID", "AcademicYearID", "GeneratedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePerformanceSummaries_SchoolID",
                table: "EmployeePerformanceSummaries",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_CurrentAcademicYearID",
                table: "EmployeeProfiles",
                column: "CurrentAcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_EmployeeJobTypeID",
                table: "EmployeeProfiles",
                column: "EmployeeJobTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_EmploymentStatus",
                table: "EmployeeProfiles",
                column: "EmploymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_IsActive",
                table: "EmployeeProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_ManagerID",
                table: "EmployeeProfiles",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_SchoolID",
                table: "EmployeeProfiles",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_SchoolID_EmployeeCode",
                table: "EmployeeProfiles",
                columns: new[] { "SchoolID", "EmployeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_SchoolStaffID",
                table: "EmployeeProfiles",
                column: "SchoolStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_TeacherID",
                table: "EmployeeProfiles",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_UserId",
                table: "EmployeeProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeQualifications_EmployeeProfileID",
                table: "EmployeeQualifications",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSpecializations_EmployeeProfileID",
                table: "EmployeeSpecializations",
                column: "EmployeeProfileID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDocuments");

            migrationBuilder.DropTable(
                name: "EmployeeHistories");

            migrationBuilder.DropTable(
                name: "EmployeeLeaves");

            migrationBuilder.DropTable(
                name: "EmployeePerformanceSummaries");

            migrationBuilder.DropTable(
                name: "EmployeeQualifications");

            migrationBuilder.DropTable(
                name: "EmployeeSpecializations");

            migrationBuilder.DropTable(
                name: "EmployeeProfiles");

            migrationBuilder.DropTable(
                name: "EmployeeJobTypes");
        }
    }
}
