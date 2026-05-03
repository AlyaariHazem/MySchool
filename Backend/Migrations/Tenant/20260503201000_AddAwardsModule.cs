using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAwardsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffAwards",
                columns: table => new
                {
                    AwardID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CycleKind = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAwards", x => x.AwardID);
                    table.ForeignKey(
                        name: "FK_StaffAwards_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffAwardCriteria",
                columns: table => new
                {
                    AwardCriteriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwardID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAwardCriteria", x => x.AwardCriteriaID);
                    table.ForeignKey(
                        name: "FK_StaffAwardCriteria_StaffAwards_AwardID",
                        column: x => x.AwardID,
                        principalTable: "StaffAwards",
                        principalColumn: "AwardID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffAwardCycles",
                columns: table => new
                {
                    AwardCycleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwardID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: true),
                    PeriodStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAwardCycles", x => x.AwardCycleID);
                    table.ForeignKey(
                        name: "FK_StaffAwardCycles_StaffAwards_AwardID",
                        column: x => x.AwardID,
                        principalTable: "StaffAwards",
                        principalColumn: "AwardID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffAwardCycles_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffAwardCycles_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffAwardNominations",
                columns: table => new
                {
                    AwardNominationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwardCycleID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    NominatedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAwardNominations", x => x.AwardNominationID);
                    table.ForeignKey(
                        name: "FK_StaffAwardNominations_EmployeeProfiles_NominatedByEmployeeProfileID",
                        column: x => x.NominatedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffAwardNominations_StaffAwardCycles_AwardCycleID",
                        column: x => x.AwardCycleID,
                        principalTable: "StaffAwardCycles",
                        principalColumn: "AwardCycleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffAwardNominations_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffAwardWinners",
                columns: table => new
                {
                    AwardWinnerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwardCycleID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    SelectedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SelectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAwardWinners", x => x.AwardWinnerID);
                    table.ForeignKey(
                        name: "FK_StaffAwardWinners_EmployeeProfiles_SelectedByEmployeeProfileID",
                        column: x => x.SelectedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffAwardWinners_StaffAwardCycles_AwardCycleID",
                        column: x => x.AwardCycleID,
                        principalTable: "StaffAwardCycles",
                        principalColumn: "AwardCycleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffAwardWinners_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardCriteria_AwardID_SortOrder",
                table: "StaffAwardCriteria",
                columns: new[] { "AwardID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardCycles_AcademicYearID",
                table: "StaffAwardCycles",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardCycles_AwardID_PeriodStartUtc_PeriodEndUtc",
                table: "StaffAwardCycles",
                columns: new[] { "AwardID", "PeriodStartUtc", "PeriodEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardCycles_TermID",
                table: "StaffAwardCycles",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardNominations_AwardCycleID_StudentID",
                table: "StaffAwardNominations",
                columns: new[] { "AwardCycleID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardNominations_NominatedByEmployeeProfileID",
                table: "StaffAwardNominations",
                column: "NominatedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardNominations_StudentID",
                table: "StaffAwardNominations",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwards_SchoolID_Code",
                table: "StaffAwards",
                columns: new[] { "SchoolID", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardWinners_AwardCycleID_Rank",
                table: "StaffAwardWinners",
                columns: new[] { "AwardCycleID", "Rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardWinners_AwardCycleID_StudentID",
                table: "StaffAwardWinners",
                columns: new[] { "AwardCycleID", "StudentID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardWinners_SelectedByEmployeeProfileID",
                table: "StaffAwardWinners",
                column: "SelectedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAwardWinners_StudentID",
                table: "StaffAwardWinners",
                column: "StudentID");

            // Default awards per school: نجم الأسبوع / الشهر / الفصل / العام
            var utc = "SYSUTCDATETIME()";
            migrationBuilder.Sql($@"
INSERT INTO StaffAwards (SchoolID, Code, Title, Description, CycleKind, IsActive, SortOrder, CreatedAtUtc, UpdatedAtUtc)
SELECT s.SchoolID, N'STAR_WEEK', N'نجم الأسبوع', NULL, 1, 1, 1, {utc}, {utc}
FROM Schools s
WHERE NOT EXISTS (SELECT 1 FROM StaffAwards a WHERE a.SchoolID = s.SchoolID AND a.Code = N'STAR_WEEK');

INSERT INTO StaffAwards (SchoolID, Code, Title, Description, CycleKind, IsActive, SortOrder, CreatedAtUtc, UpdatedAtUtc)
SELECT s.SchoolID, N'STAR_MONTH', N'نجم الشهر', NULL, 2, 1, 2, {utc}, {utc}
FROM Schools s
WHERE NOT EXISTS (SELECT 1 FROM StaffAwards a WHERE a.SchoolID = s.SchoolID AND a.Code = N'STAR_MONTH');

INSERT INTO StaffAwards (SchoolID, Code, Title, Description, CycleKind, IsActive, SortOrder, CreatedAtUtc, UpdatedAtUtc)
SELECT s.SchoolID, N'STAR_TERM', N'نجم الفصل', NULL, 3, 1, 3, {utc}, {utc}
FROM Schools s
WHERE NOT EXISTS (SELECT 1 FROM StaffAwards a WHERE a.SchoolID = s.SchoolID AND a.Code = N'STAR_TERM');

INSERT INTO StaffAwards (SchoolID, Code, Title, Description, CycleKind, IsActive, SortOrder, CreatedAtUtc, UpdatedAtUtc)
SELECT s.SchoolID, N'STAR_YEAR', N'نجم العام', NULL, 4, 1, 4, {utc}, {utc}
FROM Schools s
WHERE NOT EXISTS (SELECT 1 FROM StaffAwards a WHERE a.SchoolID = s.SchoolID AND a.Code = N'STAR_YEAR');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffAwardWinners");

            migrationBuilder.DropTable(
                name: "StaffAwardNominations");

            migrationBuilder.DropTable(
                name: "StaffAwardCriteria");

            migrationBuilder.DropTable(
                name: "StaffAwardCycles");

            migrationBuilder.DropTable(
                name: "StaffAwards");
        }
    }
}
