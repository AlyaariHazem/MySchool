using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddCentralPointsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffPointsSources",
                columns: table => new
                {
                    PointsSourceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPointsSources", x => x.PointsSourceID);
                });

            migrationBuilder.CreateTable(
                name: "StaffPointsTransactions",
                columns: table => new
                {
                    PointsTransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    PostedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedByEmployeeProfileID = table.Column<int>(type: "int", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CorrelationEntityType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CorrelationEntityID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPointsTransactions", x => x.PointsTransactionID);
                    table.ForeignKey(
                        name: "FK_StaffPointsTransactions_EmployeeProfiles_PostedByEmployeeProfileID",
                        column: x => x.PostedByEmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsTransactions_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsTransactions_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffPointsRules",
                columns: table => new
                {
                    PointsRuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: true),
                    AcademicYearID = table.Column<int>(type: "int", nullable: true),
                    PointsSourceID = table.Column<int>(type: "int", nullable: false),
                    RuleKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DeltaPoints = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPointsRules", x => x.PointsRuleID);
                    table.ForeignKey(
                        name: "FK_StaffPointsRules_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsRules_StaffPointsSources_PointsSourceID",
                        column: x => x.PointsSourceID,
                        principalTable: "StaffPointsSources",
                        principalColumn: "PointsSourceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsRules_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffPointsLedger",
                columns: table => new
                {
                    PointsLedgerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PointsTransactionID = table.Column<int>(type: "int", nullable: false),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    PointsSourceID = table.Column<int>(type: "int", nullable: false),
                    PointsRuleID = table.Column<int>(type: "int", nullable: true),
                    DeltaPoints = table.Column<int>(type: "int", nullable: false),
                    Memo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPointsLedger", x => x.PointsLedgerID);
                    table.ForeignKey(
                        name: "FK_StaffPointsLedger_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsLedger_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsLedger_StaffPointsRules_PointsRuleID",
                        column: x => x.PointsRuleID,
                        principalTable: "StaffPointsRules",
                        principalColumn: "PointsRuleID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsLedger_StaffPointsSources_PointsSourceID",
                        column: x => x.PointsSourceID,
                        principalTable: "StaffPointsSources",
                        principalColumn: "PointsSourceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsLedger_StaffPointsTransactions_PointsTransactionID",
                        column: x => x.PointsTransactionID,
                        principalTable: "StaffPointsTransactions",
                        principalColumn: "PointsTransactionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffPointsLedger_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffPointsBalanceSnapshots",
                columns: table => new
                {
                    PointsBalanceSnapshotID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeProfileID = table.Column<int>(type: "int", nullable: false),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    AcademicYearID = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPointsLedgerID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPointsBalanceSnapshots", x => x.PointsBalanceSnapshotID);
                    table.ForeignKey(
                        name: "FK_StaffPointsBalanceSnapshots_EmployeeProfiles_EmployeeProfileID",
                        column: x => x.EmployeeProfileID,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeProfileID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsBalanceSnapshots_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsBalanceSnapshots_StaffPointsLedger_LastPointsLedgerID",
                        column: x => x.LastPointsLedgerID,
                        principalTable: "StaffPointsLedger",
                        principalColumn: "PointsLedgerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPointsBalanceSnapshots_Years_AcademicYearID",
                        column: x => x.AcademicYearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsBalanceSnapshots_AcademicYearID",
                table: "StaffPointsBalanceSnapshots",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsBalanceSnapshots_EmployeeProfileID_SchoolID_AcademicYearID",
                table: "StaffPointsBalanceSnapshots",
                columns: new[] { "EmployeeProfileID", "SchoolID", "AcademicYearID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsBalanceSnapshots_LastPointsLedgerID",
                table: "StaffPointsBalanceSnapshots",
                column: "LastPointsLedgerID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsBalanceSnapshots_SchoolID",
                table: "StaffPointsBalanceSnapshots",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsLedger_AcademicYearID",
                table: "StaffPointsLedger",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsLedger_EmployeeProfileID_AcademicYearID_CreatedAtUtc",
                table: "StaffPointsLedger",
                columns: new[] { "EmployeeProfileID", "AcademicYearID", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsLedger_PointsRuleID",
                table: "StaffPointsLedger",
                column: "PointsRuleID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsLedger_PointsSourceID",
                table: "StaffPointsLedger",
                column: "PointsSourceID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsLedger_PointsTransactionID",
                table: "StaffPointsLedger",
                column: "PointsTransactionID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsLedger_SchoolID_AcademicYearID_PointsSourceID",
                table: "StaffPointsLedger",
                columns: new[] { "SchoolID", "AcademicYearID", "PointsSourceID" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsRules_AcademicYearID",
                table: "StaffPointsRules",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsRules_PointsSourceID_SchoolID_AcademicYearID_RuleKey_IsActive",
                table: "StaffPointsRules",
                columns: new[] { "PointsSourceID", "SchoolID", "AcademicYearID", "RuleKey", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsRules_SchoolID",
                table: "StaffPointsRules",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsSources_Code",
                table: "StaffPointsSources",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsTransactions_AcademicYearID",
                table: "StaffPointsTransactions",
                column: "AcademicYearID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsTransactions_IdempotencyKey",
                table: "StaffPointsTransactions",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsTransactions_PostedByEmployeeProfileID",
                table: "StaffPointsTransactions",
                column: "PostedByEmployeeProfileID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPointsTransactions_SchoolID_AcademicYearID_PostedAtUtc",
                table: "StaffPointsTransactions",
                columns: new[] { "SchoolID", "AcademicYearID", "PostedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffPointsBalanceSnapshots");

            migrationBuilder.DropTable(
                name: "StaffPointsLedger");

            migrationBuilder.DropTable(
                name: "StaffPointsRules");

            migrationBuilder.DropTable(
                name: "StaffPointsTransactions");

            migrationBuilder.DropTable(
                name: "StaffPointsSources");
        }
    }
}
