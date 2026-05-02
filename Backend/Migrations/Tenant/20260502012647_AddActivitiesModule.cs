using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddActivitiesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffActivityRequests",
                columns: table => new
                {
                    ActivityRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffActivityRequests", x => x.ActivityRequestID);
                    table.ForeignKey(
                        name: "FK_StaffActivityRequests_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffActivityRequests_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffActivityRequests_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffActivityApprovals",
                columns: table => new
                {
                    ActivityApprovalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityRequestID = table.Column<int>(type: "int", nullable: false),
                    ApproverEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffActivityApprovals", x => x.ActivityApprovalID);
                    table.ForeignKey(
                        name: "FK_StaffActivityApprovals_EmployeeProfiles_ApproverEmployeeProfileID",
                        column: x => x.ApproverEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffActivityApprovals_StaffActivityRequests_ActivityRequestID",
                        column: x => x.ActivityRequestID,
                        principalTable: "StaffActivityRequests",
                        principalColumn: "ActivityRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffActivityEvaluations",
                columns: table => new
                {
                    ActivityEvaluationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityRequestID = table.Column<int>(type: "int", nullable: false),
                    EvaluatorEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffActivityEvaluations", x => x.ActivityEvaluationID);
                    table.ForeignKey(
                        name: "FK_StaffActivityEvaluations_EmployeeProfiles_EvaluatorEmployeeProfileID",
                        column: x => x.EvaluatorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffActivityEvaluations_StaffActivityRequests_ActivityRequestID",
                        column: x => x.ActivityRequestID,
                        principalTable: "StaffActivityRequests",
                        principalColumn: "ActivityRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffActivityExecutions",
                columns: table => new
                {
                    ActivityExecutionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityRequestID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponsibleEmployeeProfileID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffActivityExecutions", x => x.ActivityExecutionID);
                    table.ForeignKey(
                        name: "FK_StaffActivityExecutions_EmployeeProfiles_ResponsibleEmployeeProfileID",
                        column: x => x.ResponsibleEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffActivityExecutions_StaffActivityRequests_ActivityRequestID",
                        column: x => x.ActivityRequestID,
                        principalTable: "StaffActivityRequests",
                        principalColumn: "ActivityRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffActivityPoints",
                columns: table => new
                {
                    ActivityPointsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityRequestID = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AwardedByEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AwardedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffActivityPoints", x => x.ActivityPointsID);
                    table.ForeignKey(
                        name: "FK_StaffActivityPoints_EmployeeProfiles_AwardedByEmployeeProfileID",
                        column: x => x.AwardedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffActivityPoints_StaffActivityRequests_ActivityRequestID",
                        column: x => x.ActivityRequestID,
                        principalTable: "StaffActivityRequests",
                        principalColumn: "ActivityRequestID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityApprovals_ActivityRequestID_SortOrder",
                table: "StaffActivityApprovals",
                columns: new[] { "ActivityRequestID", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityApprovals_ApproverEmployeeProfileID",
                table: "StaffActivityApprovals",
                column: "ApproverEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityEvaluations_ActivityRequestID_CreatedAtUtc",
                table: "StaffActivityEvaluations",
                columns: new[] { "ActivityRequestID", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityEvaluations_EvaluatorEmployeeProfileID",
                table: "StaffActivityEvaluations",
                column: "EvaluatorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityExecutions_ActivityRequestID_UpdatedAtUtc",
                table: "StaffActivityExecutions",
                columns: new[] { "ActivityRequestID", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityExecutions_ResponsibleEmployeeProfileID",
                table: "StaffActivityExecutions",
                column: "ResponsibleEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityPoints_ActivityRequestID_AwardedAtUtc",
                table: "StaffActivityPoints",
                columns: new[] { "ActivityRequestID", "AwardedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityPoints_AwardedByEmployeeProfileID",
                table: "StaffActivityPoints",
                column: "AwardedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityRequests_AcademicYearID",
                table: "StaffActivityRequests",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityRequests_EmployeeProfileID",
                table: "StaffActivityRequests",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityRequests_SchoolID_AcademicYearID_Status",
                table: "StaffActivityRequests",
                columns: new[] { "SchoolID", "AcademicYearID", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffActivityApprovals");

            migrationBuilder.DropTable(
                name: "StaffActivityEvaluations");

            migrationBuilder.DropTable(
                name: "StaffActivityExecutions");

            migrationBuilder.DropTable(
                name: "StaffActivityPoints");

            migrationBuilder.DropTable(
                name: "StaffActivityRequests");
        }
    }
}
