using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class addCurriculum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Classes_ClassID",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_ClassID",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "ClassID",
                table: "Subjects");

            migrationBuilder.CreateTable(
                name: "Curriculums",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    CurriculumName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Not = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculums", x => new { x.SubjectID, x.ClassID });
                    table.ForeignKey(
                        name: "FK_Curriculums_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Curriculums_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b938daf4-68f6-4ea6-be21-ebf3e47c5de4", new DateTime(2025, 4, 7, 16, 25, 9, 133, DateTimeKind.Local).AddTicks(1249), "AQAAAAIAAYagAAAAEH9NyqoKJUXbp5HuOe+gQe8bYF0wzRGDzgu31ObF0t24xifbv9d/a789CNIqTipkdA==", "31c22093-fe60-4d77-8aae-17da72a55b68" });

            migrationBuilder.CreateIndex(
                name: "IX_Curriculums_ClassID",
                table: "Curriculums",
                column: "ClassID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Curriculums");

            migrationBuilder.AddColumn<int>(
                name: "ClassID",
                table: "Subjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c3f10539-34b6-424f-b91b-cc4b0c2a062c", new DateTime(2025, 4, 7, 14, 58, 18, 895, DateTimeKind.Local).AddTicks(2589), "AQAAAAIAAYagAAAAEJg1EaF8bPck3j44vDiDpgsQvoV489doj5O9wPrd16tl21rAfrZ1l6PFh5jOWKvMFg==", "9ac277cb-0235-4d99-96a0-bd6da6d836c2" });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_ClassID",
                table: "Subjects",
                column: "ClassID");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Classes_ClassID",
                table: "Subjects",
                column: "ClassID",
                principalTable: "Classes",
                principalColumn: "ClassID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
