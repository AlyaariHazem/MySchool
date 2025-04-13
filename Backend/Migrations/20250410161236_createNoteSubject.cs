// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class createNoteSubject : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.AddColumn<DateTime>(
//                 name: "HireDate",
//                 table: "Subjects",
//                 type: "datetime2",
//                 nullable: false,
//                 defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

//             migrationBuilder.AddColumn<string>(
//                 name: "Note",
//                 table: "Subjects",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.AddColumn<string>(
//                 name: "SubjectReplacement",
//                 table: "Subjects",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "51bb76a6-a4d3-409e-a93d-57e76c56b0fd", new DateTime(2025, 4, 10, 19, 12, 35, 402, DateTimeKind.Local).AddTicks(6693), "AQAAAAIAAYagAAAAEMRnnbi/jpnD5nu1ZdkE5PlQQgDpOJkRT2O7m0nG5lniNBJJefc8E6lleAMuXGeDVQ==", "2f0c8581-b780-4518-8d0d-fe275ef5a6c3" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropColumn(
//                 name: "HireDate",
//                 table: "Subjects");

//             migrationBuilder.DropColumn(
//                 name: "Note",
//                 table: "Subjects");

//             migrationBuilder.DropColumn(
//                 name: "SubjectReplacement",
//                 table: "Subjects");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "b75d3120-a2d4-49a2-8ce3-0e7197d434ff", new DateTime(2025, 4, 8, 3, 34, 35, 412, DateTimeKind.Local).AddTicks(2992), "AQAAAAIAAYagAAAAEGFsw4BA5lxKxMpUyNeBj63CzTf2MkU86LX0fiV00a6gcXfbcjgdjIodNuHC7gYXzQ==", "a5234117-f727-4950-80da-4d3fedac3f6d" });
//         }
//     }
// }
