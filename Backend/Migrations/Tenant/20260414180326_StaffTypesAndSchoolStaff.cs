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
            migrationBuilder.DropIndex(
                name: "IX_HomeworkTasks_YearID",
                table: "HomeworkTasks");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeYearAssignments_YearID",
                table: "EmployeeYearAssignments");
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
