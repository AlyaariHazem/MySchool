using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant;

/// <inheritdoc />
public partial class EmployeeYearAssignments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EmployeeYearAssignments",
            columns: table => new
            {
                AssignmentID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                YearID = table.Column<int>(type: "int", nullable: false),
                EmployeeRole = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                EmployeeEntityID = table.Column<int>(type: "int", nullable: false),
                AssignmentStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                ExitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExitReason = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmployeeYearAssignments", x => x.AssignmentID);
                table.ForeignKey(
                    name: "FK_EmployeeYearAssignments_Years_YearID",
                    column: x => x.YearID,
                    principalTable: "Years",
                    principalColumn: "YearID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeYearAssignments_YearID",
            table: "EmployeeYearAssignments",
            column: "YearID");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeYearAssignments_YearID_EmployeeRole_EmployeeEntityID",
            table: "EmployeeYearAssignments",
            columns: new[] { "YearID", "EmployeeRole", "EmployeeEntityID" },
            unique: true);

        // Backfill: one Active assignment per teacher/manager for the best available academic year.
        migrationBuilder.Sql(@"
DECLARE @y INT = (SELECT TOP (1) YearID FROM Years WHERE Active = 1 ORDER BY YearID);
IF @y IS NULL SET @y = (SELECT MAX(YearID) FROM Years);
IF @y IS NOT NULL
BEGIN
    INSERT INTO EmployeeYearAssignments (YearID, EmployeeRole, EmployeeEntityID, AssignmentStatus)
    SELECT @y, N'Teacher', TeacherID, N'Active'
    FROM Teachers t
    WHERE NOT EXISTS (
        SELECT 1 FROM EmployeeYearAssignments e
        WHERE e.YearID = @y AND e.EmployeeRole = N'Teacher' AND e.EmployeeEntityID = t.TeacherID);

    INSERT INTO EmployeeYearAssignments (YearID, EmployeeRole, EmployeeEntityID, AssignmentStatus)
    SELECT @y, N'Manager', ManagerID, N'Active'
    FROM Managers m
    WHERE NOT EXISTS (
        SELECT 1 FROM EmployeeYearAssignments e
        WHERE e.YearID = @y AND e.EmployeeRole = N'Manager' AND e.EmployeeEntityID = m.ManagerID);
END
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EmployeeYearAssignments");
    }
}
