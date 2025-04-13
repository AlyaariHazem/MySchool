// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class addedDOB : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.AddColumn<DateTime>(
//                 name: "StudentDOB",
//                 table: "Students",
//                 type: "datetime2",
//                 nullable: true);

//             migrationBuilder.AddColumn<DateTime>(
//                 name: "GuardianDOB",
//                 table: "Guardians",
//                 type: "datetime2",
//                 nullable: true);
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropColumn(
//                 name: "StudentDOB",
//                 table: "Students");

//             migrationBuilder.DropColumn(
//                 name: "GuardianDOB",
//                 table: "Guardians");
//         }
//     }
// }
