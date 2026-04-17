using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddDailyEvaluationCoreModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyEvaluationTemplates",
                columns: table => new
                {
                    DailyEvaluationTemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    EmployeeJobTypeID = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyEvaluationTemplates", x => x.DailyEvaluationTemplateID);
                    table.ForeignKey(
                        name: "FK_DailyEvaluationTemplates_EmployeeJobTypes_EmployeeJobTypeID",
                        column: x => x.EmployeeJobTypeID,
                        principalTable: "EmployeeJobTypes",
                        principalColumn: "EmployeeJobTypeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DailyEvaluationTemplates_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyEvaluationTemplates_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyEvaluationCriteria",
                columns: table => new
                {
                    DailyEvaluationCriteriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyEvaluationTemplateID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MaxScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyEvaluationCriteria", x => x.DailyEvaluationCriteriaID);
                    table.ForeignKey(
                        name: "FK_DailyEvaluationCriteria_DailyEvaluationTemplates_DailyEvaluationTemplateID",
                        column: x => x.DailyEvaluationTemplateID,
                        principalTable: "DailyEvaluationTemplates",
                        principalColumn: "DailyEvaluationTemplateID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyEvaluations",
                columns: table => new
                {
                    DailyEvaluationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    EvaluatedEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    EvaluatorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EvaluatorEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    DailyEvaluationTemplateID = table.Column<int>(type: "int", nullable: false),
                    EvaluationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyEvaluations", x => x.DailyEvaluationID);
                    table.ForeignKey(
                        name: "FK_DailyEvaluations_DailyEvaluationTemplates_DailyEvaluationTemplateID",
                        column: x => x.DailyEvaluationTemplateID,
                        principalTable: "DailyEvaluationTemplates",
                        principalColumn: "DailyEvaluationTemplateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyEvaluations_EmployeeProfiles_EvaluatedEmployeeProfileID",
                        column: x => x.EvaluatedEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyEvaluations_EmployeeProfiles_EvaluatorEmployeeProfileID",
                        column: x => x.EvaluatorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyEvaluations_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyEvaluations_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationLocks",
                columns: table => new
                {
                    EvaluationLockID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    LockDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DailyEvaluationTemplateID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReopenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReopenedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationLocks", x => x.EvaluationLockID);
                    table.ForeignKey(
                        name: "FK_EvaluationLocks_DailyEvaluationTemplates_DailyEvaluationTemplateID",
                        column: x => x.DailyEvaluationTemplateID,
                        principalTable: "DailyEvaluationTemplates",
                        principalColumn: "DailyEvaluationTemplateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationLocks_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationLocks_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyEvaluationItems",
                columns: table => new
                {
                    DailyEvaluationItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyEvaluationID = table.Column<int>(type: "int", nullable: false),
                    DailyEvaluationCriteriaID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsMandatorySatisfied = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyEvaluationItems", x => x.DailyEvaluationItemID);
                    table.ForeignKey(
                        name: "FK_DailyEvaluationItems_DailyEvaluationCriteria_DailyEvaluationCriteriaID",
                        column: x => x.DailyEvaluationCriteriaID,
                        principalTable: "DailyEvaluationCriteria",
                        principalColumn: "DailyEvaluationCriteriaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyEvaluationItems_DailyEvaluations_DailyEvaluationID",
                        column: x => x.DailyEvaluationID,
                        principalTable: "DailyEvaluations",
                        principalColumn: "DailyEvaluationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationOverrideLogs",
                columns: table => new
                {
                    EvaluationOverrideLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyEvaluationID = table.Column<int>(type: "int", nullable: true),
                    EvaluationLockID = table.Column<int>(type: "int", nullable: true),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    OverrideActionType = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PreviousValuesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    NewValuesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PerformedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationOverrideLogs", x => x.EvaluationOverrideLogID);
                    table.ForeignKey(
                        name: "FK_EvaluationOverrideLogs_DailyEvaluations_DailyEvaluationID",
                        column: x => x.DailyEvaluationID,
                        principalTable: "DailyEvaluations",
                        principalColumn: "DailyEvaluationID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationOverrideLogs_EvaluationLocks_EvaluationLockID",
                        column: x => x.EvaluationLockID,
                        principalTable: "EvaluationLocks",
                        principalColumn: "EvaluationLockID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationOverrideLogs_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationOverrideLogs_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluationCriteria_DailyEvaluationTemplateID_SortOrder_IsActive",
                table: "DailyEvaluationCriteria",
                columns: new[] { "DailyEvaluationTemplateID", "SortOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluationItems_DailyEvaluationCriteriaID",
                table: "DailyEvaluationItems",
                column: "DailyEvaluationCriteriaID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluationItems_DailyEvaluationID_DailyEvaluationCriteriaID",
                table: "DailyEvaluationItems",
                columns: new[] { "DailyEvaluationID", "DailyEvaluationCriteriaID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluations_AcademicYearID",
                table: "DailyEvaluations",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluations_DailyEvaluationTemplateID",
                table: "DailyEvaluations",
                column: "DailyEvaluationTemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluations_EvaluatedEmployeeProfileID_EvaluationDate_DailyEvaluationTemplateID",
                table: "DailyEvaluations",
                columns: new[] { "EvaluatedEmployeeProfileID", "EvaluationDate", "DailyEvaluationTemplateID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluations_EvaluatorEmployeeProfileID",
                table: "DailyEvaluations",
                column: "EvaluatorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluations_SchoolID_AcademicYearID_EvaluatedEmployeeProfileID_EvaluationDate_DailyEvaluationTemplateID_IsLocked",
                table: "DailyEvaluations",
                columns: new[] { "SchoolID", "AcademicYearID", "EvaluatedEmployeeProfileID", "EvaluationDate", "DailyEvaluationTemplateID", "IsLocked" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluationTemplates_AcademicYearID",
                table: "DailyEvaluationTemplates",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluationTemplates_EmployeeJobTypeID",
                table: "DailyEvaluationTemplates",
                column: "EmployeeJobTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyEvaluationTemplates_SchoolID_AcademicYearID_EmployeeJobTypeID_Status",
                table: "DailyEvaluationTemplates",
                columns: new[] { "SchoolID", "AcademicYearID", "EmployeeJobTypeID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationLocks_AcademicYearID",
                table: "EvaluationLocks",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationLocks_DailyEvaluationTemplateID",
                table: "EvaluationLocks",
                column: "DailyEvaluationTemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationLocks_SchoolID_AcademicYearID_LockDate_DailyEvaluationTemplateID",
                table: "EvaluationLocks",
                columns: new[] { "SchoolID", "AcademicYearID", "LockDate", "DailyEvaluationTemplateID" },
                unique: true,
                filter: "[DailyEvaluationTemplateID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationLocks_SchoolID_AcademicYearID_LockDate_Status",
                table: "EvaluationLocks",
                columns: new[] { "SchoolID", "AcademicYearID", "LockDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationOverrideLogs_AcademicYearID",
                table: "EvaluationOverrideLogs",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationOverrideLogs_DailyEvaluationID_EvaluationLockID_PerformedAtUtc",
                table: "EvaluationOverrideLogs",
                columns: new[] { "DailyEvaluationID", "EvaluationLockID", "PerformedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationOverrideLogs_EvaluationLockID",
                table: "EvaluationOverrideLogs",
                column: "EvaluationLockID");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationOverrideLogs_SchoolID",
                table: "EvaluationOverrideLogs",
                column: "SchoolID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyEvaluationItems");

            migrationBuilder.DropTable(
                name: "EvaluationOverrideLogs");

            migrationBuilder.DropTable(
                name: "DailyEvaluationCriteria");

            migrationBuilder.DropTable(
                name: "DailyEvaluations");

            migrationBuilder.DropTable(
                name: "EvaluationLocks");

            migrationBuilder.DropTable(
                name: "DailyEvaluationTemplates");
        }
    }
}
