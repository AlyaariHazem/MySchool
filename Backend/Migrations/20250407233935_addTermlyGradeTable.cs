using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class addTermlyGradeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermlyGrade",
                columns: table => new
                {
                    TermlyGradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermlyGrade", x => x.TermlyGradeID);
                    table.ForeignKey(
                        name: "FK_TermlyGrade_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermlyGrade_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermlyGrade_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermlyGrade_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "31f53c41-bae7-4f1f-b99d-8a373628cdcf", new DateTime(2025, 4, 8, 2, 39, 33, 786, DateTimeKind.Local).AddTicks(8839), "AQAAAAIAAYagAAAAECL14ACA434w+ArLk27q/zD1S6HnIed1jw84c+7+2lrHX8fwpOyWlzEUMW5Di1hTcQ==", "ef96804f-9dc6-4f2c-9d27-220137e2480f" });

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrade_ClassID",
                table: "TermlyGrade",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrade_StudentID",
                table: "TermlyGrade",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrade_SubjectID",
                table: "TermlyGrade",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrade_TermID",
                table: "TermlyGrade",
                column: "TermID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TermlyGrade");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3062d71a-0f68-41e2-bfcc-4fadaabe5bf4", new DateTime(2025, 4, 7, 21, 26, 27, 976, DateTimeKind.Local).AddTicks(6393), "AQAAAAIAAYagAAAAEAuj+Ln0RWPjGXrtI7j8PHyLV3W4/+4/CTWCBnQFZotn56TJiechxcO8v1soJCTwpQ==", "e34dd158-a54c-4ada-9c7b-f0e2fc01503c" });
        }
    }
}
