using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddViolationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViolationTypes",
                columns: table => new
                {
                    ViolationTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationTypes", x => x.ViolationTypeID);
                    table.ForeignKey(
                        name: "FK_ViolationTypes_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Violations",
                columns: table => new
                {
                    ViolationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    ViolationTypeID = table.Column<int>(type: "int", nullable: false),
                    SubjectEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    OpenedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Violations", x => x.ViolationID);
                    table.ForeignKey(
                        name: "FK_Violations_EmployeeProfiles_OpenedByEmployeeProfileID",
                        column: x => x.OpenedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Violations_EmployeeProfiles_SubjectEmployeeProfileID",
                        column: x => x.SubjectEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Violations_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Violations_ViolationTypes_ViolationTypeID",
                        column: x => x.ViolationTypeID,
                        principalTable: "ViolationTypes",
                        principalColumn: "ViolationTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Violations_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ViolationActions",
                columns: table => new
                {
                    ViolationActionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViolationID = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PerformedByEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    PerformedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationActions", x => x.ViolationActionID);
                    table.ForeignKey(
                        name: "FK_ViolationActions_EmployeeProfiles_PerformedByEmployeeProfileID",
                        column: x => x.PerformedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationActions_Violations_ViolationID",
                        column: x => x.ViolationID,
                        principalTable: "Violations",
                        principalColumn: "ViolationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViolationEscalationHistories",
                columns: table => new
                {
                    ViolationEscalationHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViolationID = table.Column<int>(type: "int", nullable: false),
                    PreviousViolationTypeID = table.Column<int>(type: "int", nullable: true),
                    NewViolationTypeID = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ChangedByEmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationEscalationHistories", x => x.ViolationEscalationHistoryID);
                    table.ForeignKey(
                        name: "FK_ViolationEscalationHistories_EmployeeProfiles_ChangedByEmployeeProfileID",
                        column: x => x.ChangedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationEscalationHistories_ViolationTypes_NewViolationTypeID",
                        column: x => x.NewViolationTypeID,
                        principalTable: "ViolationTypes",
                        principalColumn: "ViolationTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationEscalationHistories_ViolationTypes_PreviousViolationTypeID",
                        column: x => x.PreviousViolationTypeID,
                        principalTable: "ViolationTypes",
                        principalColumn: "ViolationTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationEscalationHistories_Violations_ViolationID",
                        column: x => x.ViolationID,
                        principalTable: "Violations",
                        principalColumn: "ViolationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ViolationResponses",
                columns: table => new
                {
                    ViolationResponseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViolationID = table.Column<int>(type: "int", nullable: false),
                    AuthorEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationResponses", x => x.ViolationResponseID);
                    table.ForeignKey(
                        name: "FK_ViolationResponses_EmployeeProfiles_AuthorEmployeeProfileID",
                        column: x => x.AuthorEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ViolationResponses_Violations_ViolationID",
                        column: x => x.ViolationID,
                        principalTable: "Violations",
                        principalColumn: "ViolationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViolationActions_PerformedByEmployeeProfileID",
                table: "ViolationActions",
                column: "PerformedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationActions_ViolationID",
                table: "ViolationActions",
                column: "ViolationID");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEscalationHistories_ChangedByEmployeeProfileID",
                table: "ViolationEscalationHistories",
                column: "ChangedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEscalationHistories_NewViolationTypeID",
                table: "ViolationEscalationHistories",
                column: "NewViolationTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEscalationHistories_PreviousViolationTypeID",
                table: "ViolationEscalationHistories",
                column: "PreviousViolationTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEscalationHistories_ViolationID_ChangedAtUtc",
                table: "ViolationEscalationHistories",
                columns: new[] { "ViolationID", "ChangedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ViolationResponses_AuthorEmployeeProfileID",
                table: "ViolationResponses",
                column: "AuthorEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationResponses_ViolationID",
                table: "ViolationResponses",
                column: "ViolationID");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_AcademicYearID",
                table: "Violations",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_OpenedByEmployeeProfileID",
                table: "Violations",
                column: "OpenedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_SchoolID_SubjectEmployeeProfileID_Status",
                table: "Violations",
                columns: new[] { "SchoolID", "SubjectEmployeeProfileID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Violations_SubjectEmployeeProfileID",
                table: "Violations",
                column: "SubjectEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_ViolationTypeID",
                table: "Violations",
                column: "ViolationTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationTypes_SchoolID_Kind",
                table: "ViolationTypes",
                columns: new[] { "SchoolID", "Kind" },
                unique: true);

            // Seed the four escalation kinds per school (Arabic display names; UI may translate by Kind).
            migrationBuilder.Sql(
                """
                INSERT INTO ViolationTypes (SchoolID, Kind, Name, Description, SortOrder, IsActive, CreatedAtUtc, UpdatedAtUtc)
                SELECT s.SchoolID, v.Kind, v.Name, NULL, v.SortOrder, CAST(1 AS bit), SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM Schools AS s
                CROSS JOIN (VALUES
                    (0, N'استيضاح', 0),
                    (1, N'تنبيه خطي', 1),
                    (2, N'لفت نظر', 2),
                    (3, N'تنبيه نهائي', 3)
                ) AS v (Kind, Name, SortOrder);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViolationActions");

            migrationBuilder.DropTable(
                name: "ViolationEscalationHistories");

            migrationBuilder.DropTable(
                name: "ViolationResponses");

            migrationBuilder.DropTable(
                name: "Violations");

            migrationBuilder.DropTable(
                name: "ViolationTypes");
        }
    }
}
