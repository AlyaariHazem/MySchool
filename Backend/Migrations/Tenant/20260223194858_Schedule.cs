using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Schedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklySchedules",
                columns: table => new
                {
                    WeeklyScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: true),
                    TeacherID = table.Column<int>(type: "int", nullable: true),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    DivisionID = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklySchedules", x => x.WeeklyScheduleID);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "DivisionID");
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID");
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID");
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklySchedules_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_ClassID",
                table: "WeeklySchedules",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_DivisionID",
                table: "WeeklySchedules",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_SubjectID",
                table: "WeeklySchedules",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_TeacherID",
                table: "WeeklySchedules",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_TermID",
                table: "WeeklySchedules",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklySchedules_YearID",
                table: "WeeklySchedules",
                column: "YearID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklySchedules");
        }
    }
}
