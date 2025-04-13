// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class CreateTermlyGradesTable : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrade_Classes_ClassID",
//                 table: "TermlyGrade");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrade_Students_StudentID",
//                 table: "TermlyGrade");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrade_Subjects_SubjectID",
//                 table: "TermlyGrade");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrade_Terms_TermID",
//                 table: "TermlyGrade");

//             migrationBuilder.DropPrimaryKey(
//                 name: "PK_TermlyGrade",
//                 table: "TermlyGrade");

//             migrationBuilder.RenameTable(
//                 name: "TermlyGrade",
//                 newName: "TermlyGrades");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrade_TermID",
//                 table: "TermlyGrades",
//                 newName: "IX_TermlyGrades_TermID");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrade_SubjectID",
//                 table: "TermlyGrades",
//                 newName: "IX_TermlyGrades_SubjectID");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrade_StudentID",
//                 table: "TermlyGrades",
//                 newName: "IX_TermlyGrades_StudentID");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrade_ClassID",
//                 table: "TermlyGrades",
//                 newName: "IX_TermlyGrades_ClassID");

//             migrationBuilder.AddPrimaryKey(
//                 name: "PK_TermlyGrades",
//                 table: "TermlyGrades",
//                 column: "TermlyGradeID");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "b75d3120-a2d4-49a2-8ce3-0e7197d434ff", new DateTime(2025, 4, 8, 3, 34, 35, 412, DateTimeKind.Local).AddTicks(2992), "AQAAAAIAAYagAAAAEGFsw4BA5lxKxMpUyNeBj63CzTf2MkU86LX0fiV00a6gcXfbcjgdjIodNuHC7gYXzQ==", "a5234117-f727-4950-80da-4d3fedac3f6d" });

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrades_Classes_ClassID",
//                 table: "TermlyGrades",
//                 column: "ClassID",
//                 principalTable: "Classes",
//                 principalColumn: "ClassID",
//                 onDelete: ReferentialAction.Restrict);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrades_Students_StudentID",
//                 table: "TermlyGrades",
//                 column: "StudentID",
//                 principalTable: "Students",
//                 principalColumn: "StudentID",
//                 onDelete: ReferentialAction.Restrict);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrades_Subjects_SubjectID",
//                 table: "TermlyGrades",
//                 column: "SubjectID",
//                 principalTable: "Subjects",
//                 principalColumn: "SubjectID",
//                 onDelete: ReferentialAction.Restrict);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrades_Terms_TermID",
//                 table: "TermlyGrades",
//                 column: "TermID",
//                 principalTable: "Terms",
//                 principalColumn: "TermID",
//                 onDelete: ReferentialAction.Restrict);
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrades_Classes_ClassID",
//                 table: "TermlyGrades");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrades_Students_StudentID",
//                 table: "TermlyGrades");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrades_Subjects_SubjectID",
//                 table: "TermlyGrades");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_TermlyGrades_Terms_TermID",
//                 table: "TermlyGrades");

//             migrationBuilder.DropPrimaryKey(
//                 name: "PK_TermlyGrades",
//                 table: "TermlyGrades");

//             migrationBuilder.RenameTable(
//                 name: "TermlyGrades",
//                 newName: "TermlyGrade");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrades_TermID",
//                 table: "TermlyGrade",
//                 newName: "IX_TermlyGrade_TermID");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrades_SubjectID",
//                 table: "TermlyGrade",
//                 newName: "IX_TermlyGrade_SubjectID");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrades_StudentID",
//                 table: "TermlyGrade",
//                 newName: "IX_TermlyGrade_StudentID");

//             migrationBuilder.RenameIndex(
//                 name: "IX_TermlyGrades_ClassID",
//                 table: "TermlyGrade",
//                 newName: "IX_TermlyGrade_ClassID");

//             migrationBuilder.AddPrimaryKey(
//                 name: "PK_TermlyGrade",
//                 table: "TermlyGrade",
//                 column: "TermlyGradeID");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "31f53c41-bae7-4f1f-b99d-8a373628cdcf", new DateTime(2025, 4, 8, 2, 39, 33, 786, DateTimeKind.Local).AddTicks(8839), "AQAAAAIAAYagAAAAECL14ACA434w+ArLk27q/zD1S6HnIed1jw84c+7+2lrHX8fwpOyWlzEUMW5Di1hTcQ==", "ef96804f-9dc6-4f2c-9d27-220137e2480f" });

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrade_Classes_ClassID",
//                 table: "TermlyGrade",
//                 column: "ClassID",
//                 principalTable: "Classes",
//                 principalColumn: "ClassID",
//                 onDelete: ReferentialAction.Restrict);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrade_Students_StudentID",
//                 table: "TermlyGrade",
//                 column: "StudentID",
//                 principalTable: "Students",
//                 principalColumn: "StudentID",
//                 onDelete: ReferentialAction.Restrict);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrade_Subjects_SubjectID",
//                 table: "TermlyGrade",
//                 column: "SubjectID",
//                 principalTable: "Subjects",
//                 principalColumn: "SubjectID",
//                 onDelete: ReferentialAction.Restrict);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_TermlyGrade_Terms_TermID",
//                 table: "TermlyGrade",
//                 column: "TermID",
//                 principalTable: "Terms",
//                 principalColumn: "TermID",
//                 onDelete: ReferentialAction.Restrict);
//         }
//     }
// }
