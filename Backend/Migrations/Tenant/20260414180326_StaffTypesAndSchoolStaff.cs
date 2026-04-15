using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class StaffTypesAndSchoolStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: new tenant DBs may not have these indexes yet (or EmployeeYearAssignments
            // is created in a later migration). Unconditional DropIndex fails on provision.
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    WHERE i.name = N'IX_HomeworkTasks_YearID' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND t.name = N'HomeworkTasks')
    DROP INDEX [IX_HomeworkTasks_YearID] ON [dbo].[HomeworkTasks];
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    WHERE i.name = N'IX_EmployeeYearAssignments_YearID' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND t.name = N'EmployeeYearAssignments')
    DROP INDEX [IX_EmployeeYearAssignments_YearID] ON [dbo].[EmployeeYearAssignments];
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTasks_YearID",
                table: "HomeworkTasks",
                column: "YearID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeYearAssignments_YearID",
                table: "EmployeeYearAssignments",
                column: "YearID");
        }
    }
}
