using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAchievementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DefaultPoints = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementID);
                    table.ForeignKey(
                        name: "FK_Achievements_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Achievements_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AchievementRequests",
                columns: table => new
                {
                    AchievementRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AchievementID = table.Column<int>(type: "int", nullable: true),
                    CustomTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementRequests", x => x.AchievementRequestID);
                    table.ForeignKey(
                        name: "FK_AchievementRequests_Achievements_AchievementID",
                        column: x => x.AchievementID,
                        principalTable: "Achievements",
                        principalColumn: "AchievementID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementRequests_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementRequests_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementRequests_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AchievementApprovals",
                columns: table => new
                {
                    AchievementApprovalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AchievementRequestID = table.Column<int>(type: "int", nullable: false),
                    ApproverEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementApprovals", x => x.AchievementApprovalID);
                    table.ForeignKey(
                        name: "FK_AchievementApprovals_AchievementRequests_AchievementRequestID",
                        column: x => x.AchievementRequestID,
                        principalTable: "AchievementRequests",
                        principalColumn: "AchievementRequestID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchievementApprovals_EmployeeProfiles_ApproverEmployeeProfileID",
                        column: x => x.ApproverEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AchievementAttachments",
                columns: table => new
                {
                    AchievementAttachmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AchievementRequestID = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementAttachments", x => x.AchievementAttachmentID);
                    table.ForeignKey(
                        name: "FK_AchievementAttachments_AchievementRequests_AchievementRequestID",
                        column: x => x.AchievementRequestID,
                        principalTable: "AchievementRequests",
                        principalColumn: "AchievementRequestID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchievementAttachments_EmployeeProfiles_UploadedByEmployeeProfileID",
                        column: x => x.UploadedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AchievementPointsLedgers",
                columns: table => new
                {
                    AchievementPointsLedgerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    DeltaPoints = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    AchievementRequestID = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementPointsLedgers", x => x.AchievementPointsLedgerID);
                    table.ForeignKey(
                        name: "FK_AchievementPointsLedgers_AchievementRequests_AchievementRequestID",
                        column: x => x.AchievementRequestID,
                        principalTable: "AchievementRequests",
                        principalColumn: "AchievementRequestID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AchievementPointsLedgers_EmployeeProfiles_CreatedByEmployeeProfileID",
                        column: x => x.CreatedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementPointsLedgers_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementPointsLedgers_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AchievementPointsLedgers_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementApprovals_AchievementRequestID_SortOrder",
                table: "AchievementApprovals",
                columns: new[] { "AchievementRequestID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementApprovals_ApproverEmployeeProfileID",
                table: "AchievementApprovals",
                column: "ApproverEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementAttachments_AchievementRequestID",
                table: "AchievementAttachments",
                column: "AchievementRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementAttachments_UploadedByEmployeeProfileID",
                table: "AchievementAttachments",
                column: "UploadedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementPointsLedgers_AcademicYearID",
                table: "AchievementPointsLedgers",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementPointsLedgers_AchievementRequestID",
                table: "AchievementPointsLedgers",
                column: "AchievementRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementPointsLedgers_CreatedByEmployeeProfileID",
                table: "AchievementPointsLedgers",
                column: "CreatedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementPointsLedgers_EmployeeProfileID_AcademicYearID_CreatedAtUtc",
                table: "AchievementPointsLedgers",
                columns: new[] { "EmployeeProfileID", "AcademicYearID", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementPointsLedgers_SchoolID",
                table: "AchievementPointsLedgers",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementRequests_AcademicYearID",
                table: "AchievementRequests",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementRequests_AchievementID",
                table: "AchievementRequests",
                column: "AchievementID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementRequests_EmployeeProfileID",
                table: "AchievementRequests",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementRequests_SchoolID_AcademicYearID_EmployeeProfileID_Status",
                table: "AchievementRequests",
                columns: new[] { "SchoolID", "AcademicYearID", "EmployeeProfileID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_AcademicYearID",
                table: "Achievements",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_SchoolID_Code",
                table: "Achievements",
                columns: new[] { "SchoolID", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementApprovals");

            migrationBuilder.DropTable(
                name: "AchievementAttachments");

            migrationBuilder.DropTable(
                name: "AchievementPointsLedgers");

            migrationBuilder.DropTable(
                name: "AchievementRequests");

            migrationBuilder.DropTable(
                name: "Achievements");
        }
    }
}
