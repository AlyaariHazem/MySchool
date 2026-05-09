using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentAnalytics",
                columns: table => new
                {
                    DepartmentAnalyticsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    TermID = table.Column<int>(type: "int", nullable: true),
                    PeriodKind = table.Column<int>(type: "int", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KpiCount = table.Column<int>(type: "int", nullable: false),
                    AverageScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TargetAchievementPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentAnalytics", x => x.DepartmentAnalyticsID);
                    table.ForeignKey(
                        name: "FK_DepartmentAnalytics_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentAnalytics_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentAnalytics_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KpiDefinitions",
                columns: table => new
                {
                    KpiDefinitionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    HigherIsBetter = table.Column<bool>(type: "bit", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiDefinitions", x => x.KpiDefinitionID);
                    table.ForeignKey(
                        name: "FK_KpiDefinitions_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolAnalytics",
                columns: table => new
                {
                    SchoolAnalyticsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    TermID = table.Column<int>(type: "int", nullable: true),
                    PeriodKind = table.Column<int>(type: "int", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KpiCount = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TargetAchievementPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolAnalytics", x => x.SchoolAnalyticsID);
                    table.ForeignKey(
                        name: "FK_SchoolAnalytics_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolAnalytics_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolAnalytics_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAnalytics",
                columns: table => new
                {
                    TeacherAnalyticsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    TermID = table.Column<int>(type: "int", nullable: true),
                    PeriodKind = table.Column<int>(type: "int", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KpiCount = table.Column<int>(type: "int", nullable: false),
                    CompositeScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TargetAchievementPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAnalytics", x => x.TeacherAnalyticsID);
                    table.ForeignKey(
                        name: "FK_TeacherAnalytics_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherAnalytics_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherAnalytics_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherAnalytics_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KpiSnapshots",
                columns: table => new
                {
                    KpiSnapshotID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiDefinitionID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    TermID = table.Column<int>(type: "int", nullable: true),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PeriodKind = table.Column<int>(type: "int", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiSnapshots", x => x.KpiSnapshotID);
                    table.ForeignKey(
                        name: "FK_KpiSnapshots_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpiSnapshots_KpiDefinitions_KpiDefinitionID",
                        column: x => x.KpiDefinitionID,
                        principalTable: "KpiDefinitions",
                        principalColumn: "KpiDefinitionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KpiSnapshots_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpiSnapshots_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpiSnapshots_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrendAnalysis",
                columns: table => new
                {
                    TrendAnalysisID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    KpiDefinitionID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    DashboardAudience = table.Column<int>(type: "int", nullable: false),
                    PeriodKind = table.Column<int>(type: "int", nullable: false),
                    FromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaselineValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeltaValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeltaPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPositiveTrend = table.Column<bool>(type: "bit", nullable: false),
                    TrendLabel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ComputedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendAnalysis", x => x.TrendAnalysisID);
                    table.ForeignKey(
                        name: "FK_TrendAnalysis_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrendAnalysis_KpiDefinitions_KpiDefinitionID",
                        column: x => x.KpiDefinitionID,
                        principalTable: "KpiDefinitions",
                        principalColumn: "KpiDefinitionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrendAnalysis_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrendAnalysis_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAnalytics_AcademicYearID",
                table: "DepartmentAnalytics",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAnalytics_SchoolID_DepartmentName_PeriodStartUtc_PeriodEndUtc",
                table: "DepartmentAnalytics",
                columns: new[] { "SchoolID", "DepartmentName", "PeriodStartUtc", "PeriodEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAnalytics_TermID",
                table: "DepartmentAnalytics",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_KpiDefinitions_SchoolID_Code",
                table: "KpiDefinitions",
                columns: new[] { "SchoolID", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpiSnapshots_AcademicYearID",
                table: "KpiSnapshots",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_KpiSnapshots_EmployeeProfileID",
                table: "KpiSnapshots",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_KpiSnapshots_KpiDefinitionID_SchoolID_PeriodStartUtc_PeriodEndUtc",
                table: "KpiSnapshots",
                columns: new[] { "KpiDefinitionID", "SchoolID", "PeriodStartUtc", "PeriodEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_KpiSnapshots_SchoolID",
                table: "KpiSnapshots",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_KpiSnapshots_TermID",
                table: "KpiSnapshots",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAnalytics_AcademicYearID",
                table: "SchoolAnalytics",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAnalytics_SchoolID_PeriodStartUtc_PeriodEndUtc",
                table: "SchoolAnalytics",
                columns: new[] { "SchoolID", "PeriodStartUtc", "PeriodEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAnalytics_TermID",
                table: "SchoolAnalytics",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAnalytics_AcademicYearID",
                table: "TeacherAnalytics",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAnalytics_EmployeeProfileID",
                table: "TeacherAnalytics",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAnalytics_SchoolID_EmployeeProfileID_PeriodStartUtc_PeriodEndUtc",
                table: "TeacherAnalytics",
                columns: new[] { "SchoolID", "EmployeeProfileID", "PeriodStartUtc", "PeriodEndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAnalytics_TermID",
                table: "TeacherAnalytics",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_TrendAnalysis_AcademicYearID",
                table: "TrendAnalysis",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_TrendAnalysis_EmployeeProfileID",
                table: "TrendAnalysis",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_TrendAnalysis_KpiDefinitionID",
                table: "TrendAnalysis",
                column: "KpiDefinitionID");

            migrationBuilder.CreateIndex(
                name: "IX_TrendAnalysis_SchoolID_KpiDefinitionID_DashboardAudience_FromUtc_ToUtc",
                table: "TrendAnalysis",
                columns: new[] { "SchoolID", "KpiDefinitionID", "DashboardAudience", "FromUtc", "ToUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentAnalytics");

            migrationBuilder.DropTable(
                name: "KpiSnapshots");

            migrationBuilder.DropTable(
                name: "SchoolAnalytics");

            migrationBuilder.DropTable(
                name: "TeacherAnalytics");

            migrationBuilder.DropTable(
                name: "TrendAnalysis");

            migrationBuilder.DropTable(
                name: "KpiDefinitions");
        }
    }
}
