using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddMeetingsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffMeetings",
                columns: table => new
                {
                    MeetingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    OrganizerEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    StartAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMeetings", x => x.MeetingID);
                    table.ForeignKey(
                        name: "FK_StaffMeetings_EmployeeProfiles_OrganizerEmployeeProfileID",
                        column: x => x.OrganizerEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffMeetings_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffMeetings_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffMeetingAttendees",
                columns: table => new
                {
                    MeetingAttendeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingID = table.Column<int>(type: "int", nullable: false),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Response = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMeetingAttendees", x => x.MeetingAttendeeID);
                    table.ForeignKey(
                        name: "FK_StaffMeetingAttendees_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffMeetingAttendees_StaffMeetings_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "StaffMeetings",
                        principalColumn: "MeetingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffMeetingMinutes",
                columns: table => new
                {
                    MeetingMinutesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingID = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedByEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMeetingMinutes", x => x.MeetingMinutesID);
                    table.ForeignKey(
                        name: "FK_StaffMeetingMinutes_EmployeeProfiles_ApprovedByEmployeeProfileID",
                        column: x => x.ApprovedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffMeetingMinutes_EmployeeProfiles_RecordedByEmployeeProfileID",
                        column: x => x.RecordedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffMeetingMinutes_StaffMeetings_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "StaffMeetings",
                        principalColumn: "MeetingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffMeetingTasks",
                columns: table => new
                {
                    MeetingTaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AssignedToEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMeetingTasks", x => x.MeetingTaskID);
                    table.ForeignKey(
                        name: "FK_StaffMeetingTasks_EmployeeProfiles_AssignedToEmployeeProfileID",
                        column: x => x.AssignedToEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffMeetingTasks_StaffMeetings_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "StaffMeetings",
                        principalColumn: "MeetingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffMeetingTaskFollowUps",
                columns: table => new
                {
                    MeetingTaskFollowUpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingTaskID = table.Column<int>(type: "int", nullable: false),
                    AuthorEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMeetingTaskFollowUps", x => x.MeetingTaskFollowUpID);
                    table.ForeignKey(
                        name: "FK_StaffMeetingTaskFollowUps_EmployeeProfiles_AuthorEmployeeProfileID",
                        column: x => x.AuthorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffMeetingTaskFollowUps_StaffMeetingTasks_MeetingTaskID",
                        column: x => x.MeetingTaskID,
                        principalTable: "StaffMeetingTasks",
                        principalColumn: "MeetingTaskID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingAttendees_EmployeeProfileID",
                table: "StaffMeetingAttendees",
                column: "EmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingAttendees_MeetingID_EmployeeProfileID",
                table: "StaffMeetingAttendees",
                columns: new[] { "MeetingID", "EmployeeProfileID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingMinutes_ApprovedByEmployeeProfileID",
                table: "StaffMeetingMinutes",
                column: "ApprovedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingMinutes_MeetingID",
                table: "StaffMeetingMinutes",
                column: "MeetingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingMinutes_RecordedByEmployeeProfileID",
                table: "StaffMeetingMinutes",
                column: "RecordedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetings_AcademicYearID",
                table: "StaffMeetings",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetings_OrganizerEmployeeProfileID",
                table: "StaffMeetings",
                column: "OrganizerEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetings_SchoolID_AcademicYearID_Status",
                table: "StaffMeetings",
                columns: new[] { "SchoolID", "AcademicYearID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetings_SchoolID_StartAtUtc",
                table: "StaffMeetings",
                columns: new[] { "SchoolID", "StartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingTaskFollowUps_AuthorEmployeeProfileID",
                table: "StaffMeetingTaskFollowUps",
                column: "AuthorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingTaskFollowUps_MeetingTaskID_CreatedAtUtc",
                table: "StaffMeetingTaskFollowUps",
                columns: new[] { "MeetingTaskID", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingTasks_AssignedToEmployeeProfileID",
                table: "StaffMeetingTasks",
                column: "AssignedToEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMeetingTasks_MeetingID_SortOrder",
                table: "StaffMeetingTasks",
                columns: new[] { "MeetingID", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffMeetingAttendees");

            migrationBuilder.DropTable(
                name: "StaffMeetingMinutes");

            migrationBuilder.DropTable(
                name: "StaffMeetingTaskFollowUps");

            migrationBuilder.DropTable(
                name: "StaffMeetingTasks");

            migrationBuilder.DropTable(
                name: "StaffMeetings");
        }
    }
}
