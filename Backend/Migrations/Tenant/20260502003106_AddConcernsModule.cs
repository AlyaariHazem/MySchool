using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddConcernsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffConcernCategories",
                columns: table => new
                {
                    ConcernCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CategoryKind = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffConcernCategories", x => x.ConcernCategoryID);
                    table.ForeignKey(
                        name: "FK_StaffConcernCategories_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffComplaints",
                columns: table => new
                {
                    ComplaintID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    ConcernCategoryID = table.Column<int>(type: "int", nullable: false),
                    SubmitterEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AssignedToEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffComplaints", x => x.ComplaintID);
                    table.ForeignKey(
                        name: "FK_StaffComplaints_EmployeeProfiles_AssignedToEmployeeProfileID",
                        column: x => x.AssignedToEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffComplaints_EmployeeProfiles_SubmitterEmployeeProfileID",
                        column: x => x.SubmitterEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffComplaints_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffComplaints_StaffConcernCategories_ConcernCategoryID",
                        column: x => x.ConcernCategoryID,
                        principalTable: "StaffConcernCategories",
                        principalColumn: "ConcernCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffComplaints_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffSuggestions",
                columns: table => new
                {
                    SuggestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    ConcernCategoryID = table.Column<int>(type: "int", nullable: false),
                    SubmitterEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    AssignedToEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffSuggestions", x => x.SuggestionID);
                    table.ForeignKey(
                        name: "FK_StaffSuggestions_EmployeeProfiles_AssignedToEmployeeProfileID",
                        column: x => x.AssignedToEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffSuggestions_EmployeeProfiles_SubmitterEmployeeProfileID",
                        column: x => x.SubmitterEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffSuggestions_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffSuggestions_StaffConcernCategories_ConcernCategoryID",
                        column: x => x.ConcernCategoryID,
                        principalTable: "StaffConcernCategories",
                        principalColumn: "ConcernCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffSuggestions_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffConcernActionLogs",
                columns: table => new
                {
                    ConcernActionLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintID = table.Column<int>(type: "int", nullable: true),
                    SuggestionID = table.Column<int>(type: "int", nullable: true),
                    ActorEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    ActionKind = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffConcernActionLogs", x => x.ConcernActionLogID);
                    table.ForeignKey(
                        name: "FK_StaffConcernActionLogs_EmployeeProfiles_ActorEmployeeProfileID",
                        column: x => x.ActorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffConcernActionLogs_StaffComplaints_ComplaintID",
                        column: x => x.ComplaintID,
                        principalTable: "StaffComplaints",
                        principalColumn: "ComplaintID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffConcernActionLogs_StaffSuggestions_SuggestionID",
                        column: x => x.SuggestionID,
                        principalTable: "StaffSuggestions",
                        principalColumn: "SuggestionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplaints_AcademicYearID",
                table: "StaffComplaints",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplaints_AssignedToEmployeeProfileID",
                table: "StaffComplaints",
                column: "AssignedToEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplaints_ConcernCategoryID",
                table: "StaffComplaints",
                column: "ConcernCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplaints_SchoolID_AcademicYearID_Status",
                table: "StaffComplaints",
                columns: new[] { "SchoolID", "AcademicYearID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffComplaints_SubmitterEmployeeProfileID",
                table: "StaffComplaints",
                column: "SubmitterEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffConcernActionLogs_ActorEmployeeProfileID",
                table: "StaffConcernActionLogs",
                column: "ActorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffConcernActionLogs_ComplaintID",
                table: "StaffConcernActionLogs",
                column: "ComplaintID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffConcernActionLogs_SuggestionID",
                table: "StaffConcernActionLogs",
                column: "SuggestionID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffConcernCategories_SchoolID_Code",
                table: "StaffConcernCategories",
                columns: new[] { "SchoolID", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffSuggestions_AcademicYearID",
                table: "StaffSuggestions",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSuggestions_AssignedToEmployeeProfileID",
                table: "StaffSuggestions",
                column: "AssignedToEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSuggestions_ConcernCategoryID",
                table: "StaffSuggestions",
                column: "ConcernCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSuggestions_SchoolID_AcademicYearID_Status",
                table: "StaffSuggestions",
                columns: new[] { "SchoolID", "AcademicYearID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffSuggestions_SubmitterEmployeeProfileID",
                table: "StaffSuggestions",
                column: "SubmitterEmployeeProfileID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffConcernActionLogs");

            migrationBuilder.DropTable(
                name: "StaffComplaints");

            migrationBuilder.DropTable(
                name: "StaffSuggestions");

            migrationBuilder.DropTable(
                name: "StaffConcernCategories");
        }
    }
}
