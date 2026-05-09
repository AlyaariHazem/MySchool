using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddTimeCapsuleModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResignationRequests",
                columns: table => new
                {
                    ResignationRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    RequestDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResignationRequests", x => x.ResignationRequestID);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeCapsules",
                columns: table => new
                {
                    TimeCapsuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    UnlockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnlockedByUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UnlockReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeCapsules", x => x.TimeCapsuleID);
                    table.ForeignKey(
                        name: "FK_TimeCapsules_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeCapsules_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CapsuleAccessLogs",
                columns: table => new
                {
                    CapsuleAccessLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeCapsuleID = table.Column<int>(type: "int", nullable: false),
                    AccessedByUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AccessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapsuleAccessLogs", x => x.CapsuleAccessLogID);
                    table.ForeignKey(
                        name: "FK_CapsuleAccessLogs_TimeCapsules_TimeCapsuleID",
                        column: x => x.TimeCapsuleID,
                        principalTable: "TimeCapsules",
                        principalColumn: "TimeCapsuleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapsuleNarratives",
                columns: table => new
                {
                    CapsuleNarrativeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeCapsuleID = table.Column<int>(type: "int", nullable: false),
                    NarrativeText = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapsuleNarratives", x => x.CapsuleNarrativeID);
                    table.ForeignKey(
                        name: "FK_CapsuleNarratives_TimeCapsules_TimeCapsuleID",
                        column: x => x.TimeCapsuleID,
                        principalTable: "TimeCapsules",
                        principalColumn: "TimeCapsuleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapsuleUnlockApprovals",
                columns: table => new
                {
                    CapsuleUnlockApprovalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeCapsuleID = table.Column<int>(type: "int", nullable: false),
                    ResignationRequestID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapsuleUnlockApprovals", x => x.CapsuleUnlockApprovalID);
                    table.ForeignKey(
                        name: "FK_CapsuleUnlockApprovals_ResignationRequests_ResignationRequestID",
                        column: x => x.ResignationRequestID,
                        principalTable: "ResignationRequests",
                        principalColumn: "ResignationRequestID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CapsuleUnlockApprovals_TimeCapsules_TimeCapsuleID",
                        column: x => x.TimeCapsuleID,
                        principalTable: "TimeCapsules",
                        principalColumn: "TimeCapsuleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeCapsuleSections",
                columns: table => new
                {
                    TimeCapsuleSectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeCapsuleID = table.Column<int>(type: "int", nullable: false),
                    SectionType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeCapsuleSections", x => x.TimeCapsuleSectionID);
                    table.ForeignKey(
                        name: "FK_TimeCapsuleSections_TimeCapsules_TimeCapsuleID",
                        column: x => x.TimeCapsuleID,
                        principalTable: "TimeCapsules",
                        principalColumn: "TimeCapsuleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleAccessLogs_TimeCapsuleID_AccessedAtUtc",
                table: "CapsuleAccessLogs",
                columns: new[] { "TimeCapsuleID", "AccessedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleNarratives_TimeCapsuleID",
                table: "CapsuleNarratives",
                column: "TimeCapsuleID");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleUnlockApprovals_ResignationRequestID",
                table: "CapsuleUnlockApprovals",
                column: "ResignationRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleUnlockApprovals_TimeCapsuleID_Status",
                table: "CapsuleUnlockApprovals",
                columns: new[] { "TimeCapsuleID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_AcademicYearID",
                table: "ResignationRequests",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_EmployeeProfileID_Status",
                table: "ResignationRequests",
                columns: new[] { "EmployeeProfileID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_SchoolID",
                table: "ResignationRequests",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_TimeCapsules_EmployeeProfileID",
                table: "TimeCapsules",
                column: "EmployeeProfileID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeCapsules_SchoolID",
                table: "TimeCapsules",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_TimeCapsuleSections_TimeCapsuleID_SectionType",
                table: "TimeCapsuleSections",
                columns: new[] { "TimeCapsuleID", "SectionType" },
                unique: true);

            // Existing employee profiles (pre-module) get a locked capsule row.
            migrationBuilder.Sql(
                """
                INSERT INTO TimeCapsules (EmployeeProfileID, SchoolID, IsLocked, CreatedAtUtc, IsActive)
                SELECT ep.EmployeeProfileID, ep.SchoolID, CAST(1 AS bit), SYSUTCDATETIME(), CAST(1 AS bit)
                FROM EmployeeProfiles ep
                WHERE NOT EXISTS (
                    SELECT 1 FROM TimeCapsules tc WHERE tc.EmployeeProfileID = ep.EmployeeProfileID);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapsuleAccessLogs");

            migrationBuilder.DropTable(
                name: "CapsuleNarratives");

            migrationBuilder.DropTable(
                name: "CapsuleUnlockApprovals");

            migrationBuilder.DropTable(
                name: "TimeCapsuleSections");

            migrationBuilder.DropTable(
                name: "ResignationRequests");

            migrationBuilder.DropTable(
                name: "TimeCapsules");
        }
    }
}
