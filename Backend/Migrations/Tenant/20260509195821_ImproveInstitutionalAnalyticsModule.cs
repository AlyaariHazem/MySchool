using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class ImproveInstitutionalAnalyticsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "KpiDefinitionID",
                table: "TrendAnalysis",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EntityID",
                table: "TrendAnalysis",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityType",
                table: "TrendAnalysis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Interpretation",
                table: "TrendAnalysis",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetricCode",
                table: "TrendAnalysis",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrendDirection",
                table: "TrendAnalysis",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AchievementPoints",
                table: "TeacherAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ActivityCount",
                table: "TeacherAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AttendanceOrDisciplineScore",
                table: "TeacherAnalytics",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageDailyEvaluationScore",
                table: "TeacherAnalytics",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComplaintCount",
                table: "TeacherAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PerformanceLevel",
                table: "TeacherAnalytics",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recommendations",
                table: "TeacherAnalytics",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StrengthsSummary",
                table: "TeacherAnalytics",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SupervisorVisitAverage",
                table: "TeacherAnalytics",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrendDirection",
                table: "TeacherAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViolationPoints",
                table: "TeacherAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WeaknessesSummary",
                table: "TeacherAnalytics",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveTeacherCount",
                table: "SchoolAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageTeacherScore",
                table: "SchoolAnalytics",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "SchoolAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanCompletionPercent",
                table: "SchoolAnalytics",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskLevel",
                table: "SchoolAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TotalAchievements",
                table: "SchoolAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalActivities",
                table: "SchoolAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalComplaints",
                table: "SchoolAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalViolations",
                table: "SchoolAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ChangePercent",
                table: "KpiSnapshots",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "KpiSnapshots",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousValue",
                table: "KpiSnapshots",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SnapshotDateUtc",
                table: "KpiSnapshots",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE KpiSnapshots SET SnapshotDateUtc = RecordedAtUtc WHERE SnapshotDateUtc IS NULL;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SnapshotDateUtc",
                table: "KpiSnapshots",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "KpiSnapshots",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ArabicName",
                table: "KpiDefinitions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CalculationType",
                table: "KpiDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "KpiDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "KpiDefinitions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemKpi",
                table: "KpiDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "KpiDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetAudience",
                table: "KpiDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AchievementCount",
                table: "DepartmentAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ActivityCount",
                table: "DepartmentAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ComplaintCount",
                table: "DepartmentAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCount",
                table: "DepartmentAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeJobTypeID",
                table: "DepartmentAnalytics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformanceLevel",
                table: "DepartmentAnalytics",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanCompletionPercent",
                table: "DepartmentAnalytics",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViolationCount",
                table: "DepartmentAnalytics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrendAnalysis_SchoolID_MetricCode_PeriodKind_FromUtc_ToUtc",
                table: "TrendAnalysis",
                columns: new[] { "SchoolID", "MetricCode", "PeriodKind", "FromUtc", "ToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAnalytics_EmployeeJobTypeID",
                table: "DepartmentAnalytics",
                column: "EmployeeJobTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAnalytics_SchoolID_EmployeeJobTypeID_PeriodStartUtc_PeriodEndUtc",
                table: "DepartmentAnalytics",
                columns: new[] { "SchoolID", "EmployeeJobTypeID", "PeriodStartUtc", "PeriodEndUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentAnalytics_EmployeeJobTypes_EmployeeJobTypeID",
                table: "DepartmentAnalytics",
                column: "EmployeeJobTypeID",
                principalTable: "EmployeeJobTypes",
                principalColumn: "EmployeeJobTypeID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentAnalytics_EmployeeJobTypes_EmployeeJobTypeID",
                table: "DepartmentAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_TrendAnalysis_SchoolID_MetricCode_PeriodKind_FromUtc_ToUtc",
                table: "TrendAnalysis");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentAnalytics_EmployeeJobTypeID",
                table: "DepartmentAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentAnalytics_SchoolID_EmployeeJobTypeID_PeriodStartUtc_PeriodEndUtc",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "EntityID",
                table: "TrendAnalysis");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "TrendAnalysis");

            migrationBuilder.DropColumn(
                name: "Interpretation",
                table: "TrendAnalysis");

            migrationBuilder.DropColumn(
                name: "MetricCode",
                table: "TrendAnalysis");

            migrationBuilder.DropColumn(
                name: "TrendDirection",
                table: "TrendAnalysis");

            migrationBuilder.DropColumn(
                name: "AchievementPoints",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "ActivityCount",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "AttendanceOrDisciplineScore",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "AverageDailyEvaluationScore",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "ComplaintCount",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "PerformanceLevel",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "Recommendations",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "StrengthsSummary",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "SupervisorVisitAverage",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "TrendDirection",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "ViolationPoints",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "WeaknessesSummary",
                table: "TeacherAnalytics");

            migrationBuilder.DropColumn(
                name: "ActiveTeacherCount",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "AverageTeacherScore",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "PlanCompletionPercent",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "TotalAchievements",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "TotalActivities",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "TotalComplaints",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "TotalViolations",
                table: "SchoolAnalytics");

            migrationBuilder.DropColumn(
                name: "ChangePercent",
                table: "KpiSnapshots");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "KpiSnapshots");

            migrationBuilder.DropColumn(
                name: "PreviousValue",
                table: "KpiSnapshots");

            migrationBuilder.DropColumn(
                name: "SnapshotDateUtc",
                table: "KpiSnapshots");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "KpiSnapshots");

            migrationBuilder.DropColumn(
                name: "ArabicName",
                table: "KpiDefinitions");

            migrationBuilder.DropColumn(
                name: "CalculationType",
                table: "KpiDefinitions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "KpiDefinitions");

            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "KpiDefinitions");

            migrationBuilder.DropColumn(
                name: "IsSystemKpi",
                table: "KpiDefinitions");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "KpiDefinitions");

            migrationBuilder.DropColumn(
                name: "TargetAudience",
                table: "KpiDefinitions");

            migrationBuilder.DropColumn(
                name: "AchievementCount",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "ActivityCount",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "ComplaintCount",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "EmployeeCount",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "EmployeeJobTypeID",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "PerformanceLevel",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "PlanCompletionPercent",
                table: "DepartmentAnalytics");

            migrationBuilder.DropColumn(
                name: "ViolationCount",
                table: "DepartmentAnalytics");

            migrationBuilder.AlterColumn<int>(
                name: "KpiDefinitionID",
                table: "TrendAnalysis",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
