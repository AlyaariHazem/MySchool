using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddOrganizationalPlansModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffStrategicGoals",
                columns: table => new
                {
                    StrategicGoalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffStrategicGoals", x => x.StrategicGoalID);
                    table.ForeignKey(
                        name: "FK_StaffStrategicGoals_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffAnnualGoals",
                columns: table => new
                {
                    AnnualGoalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    StrategicGoalID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAnnualGoals", x => x.AnnualGoalID);
                    table.ForeignKey(
                        name: "FK_StaffAnnualGoals_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffAnnualGoals_StaffStrategicGoals_StrategicGoalID",
                        column: x => x.StrategicGoalID,
                        principalTable: "StaffStrategicGoals",
                        principalColumn: "StrategicGoalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffAnnualGoals_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffDepartmentGoals",
                columns: table => new
                {
                    DepartmentGoalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    StrategicGoalID = table.Column<int>(type: "int", nullable: true),
                    AnnualGoalID = table.Column<int>(type: "int", nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    OwnerEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffDepartmentGoals", x => x.DepartmentGoalID);
                    table.ForeignKey(
                        name: "FK_StaffDepartmentGoals_EmployeeProfiles_OwnerEmployeeProfileID",
                        column: x => x.OwnerEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffDepartmentGoals_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffDepartmentGoals_StaffAnnualGoals_AnnualGoalID",
                        column: x => x.AnnualGoalID,
                        principalTable: "StaffAnnualGoals",
                        principalColumn: "AnnualGoalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffDepartmentGoals_StaffStrategicGoals_StrategicGoalID",
                        column: x => x.StrategicGoalID,
                        principalTable: "StaffStrategicGoals",
                        principalColumn: "StrategicGoalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffDepartmentGoals_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffOperationalPlans",
                columns: table => new
                {
                    OperationalPlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnualGoalID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffOperationalPlans", x => x.OperationalPlanID);
                    table.ForeignKey(
                        name: "FK_StaffOperationalPlans_EmployeeProfiles_OwnerEmployeeProfileID",
                        column: x => x.OwnerEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffOperationalPlans_StaffAnnualGoals_AnnualGoalID",
                        column: x => x.AnnualGoalID,
                        principalTable: "StaffAnnualGoals",
                        principalColumn: "AnnualGoalID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffPlanTasks",
                columns: table => new
                {
                    PlanTaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationalPlanID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedToEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPlanTasks", x => x.PlanTaskID);
                    table.ForeignKey(
                        name: "FK_StaffPlanTasks_EmployeeProfiles_AssignedToEmployeeProfileID",
                        column: x => x.AssignedToEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPlanTasks_StaffOperationalPlans_OperationalPlanID",
                        column: x => x.OperationalPlanID,
                        principalTable: "StaffOperationalPlans",
                        principalColumn: "OperationalPlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffPlanProgressUpdates",
                columns: table => new
                {
                    PlanProgressUpdateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanTaskID = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProgressPercent = table.Column<int>(type: "int", nullable: true),
                    AuthorEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPlanProgressUpdates", x => x.PlanProgressUpdateID);
                    table.ForeignKey(
                        name: "FK_StaffPlanProgressUpdates_EmployeeProfiles_AuthorEmployeeProfileID",
                        column: x => x.AuthorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPlanProgressUpdates_StaffPlanTasks_PlanTaskID",
                        column: x => x.PlanTaskID,
                        principalTable: "StaffPlanTasks",
                        principalColumn: "PlanTaskID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAnnualGoals_AcademicYearID",
                table: "StaffAnnualGoals",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAnnualGoals_SchoolID_AcademicYearID_Status",
                table: "StaffAnnualGoals",
                columns: new[] { "SchoolID", "AcademicYearID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAnnualGoals_StrategicGoalID_SortOrder",
                table: "StaffAnnualGoals",
                columns: new[] { "StrategicGoalID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartmentGoals_AcademicYearID",
                table: "StaffDepartmentGoals",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartmentGoals_AnnualGoalID",
                table: "StaffDepartmentGoals",
                column: "AnnualGoalID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartmentGoals_OwnerEmployeeProfileID",
                table: "StaffDepartmentGoals",
                column: "OwnerEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartmentGoals_SchoolID_AcademicYearID_Status",
                table: "StaffDepartmentGoals",
                columns: new[] { "SchoolID", "AcademicYearID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartmentGoals_SchoolID_DepartmentName",
                table: "StaffDepartmentGoals",
                columns: new[] { "SchoolID", "DepartmentName" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffDepartmentGoals_StrategicGoalID",
                table: "StaffDepartmentGoals",
                column: "StrategicGoalID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffOperationalPlans_AnnualGoalID_SortOrder",
                table: "StaffOperationalPlans",
                columns: new[] { "AnnualGoalID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffOperationalPlans_OwnerEmployeeProfileID",
                table: "StaffOperationalPlans",
                column: "OwnerEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlanProgressUpdates_AuthorEmployeeProfileID",
                table: "StaffPlanProgressUpdates",
                column: "AuthorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlanProgressUpdates_PlanTaskID_CreatedAtUtc",
                table: "StaffPlanProgressUpdates",
                columns: new[] { "PlanTaskID", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlanTasks_AssignedToEmployeeProfileID",
                table: "StaffPlanTasks",
                column: "AssignedToEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlanTasks_OperationalPlanID_DueAtUtc",
                table: "StaffPlanTasks",
                columns: new[] { "OperationalPlanID", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffPlanTasks_OperationalPlanID_SortOrder",
                table: "StaffPlanTasks",
                columns: new[] { "OperationalPlanID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffStrategicGoals_SchoolID_SortOrder",
                table: "StaffStrategicGoals",
                columns: new[] { "SchoolID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffStrategicGoals_SchoolID_Status",
                table: "StaffStrategicGoals",
                columns: new[] { "SchoolID", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffDepartmentGoals");

            migrationBuilder.DropTable(
                name: "StaffPlanProgressUpdates");

            migrationBuilder.DropTable(
                name: "StaffPlanTasks");

            migrationBuilder.DropTable(
                name: "StaffOperationalPlans");

            migrationBuilder.DropTable(
                name: "StaffAnnualGoals");

            migrationBuilder.DropTable(
                name: "StaffStrategicGoals");
        }
    }
}
