// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class editNoteInCurriculum : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.RenameColumn(
//                 name: "Not",
//                 table: "Curriculums",
//                 newName: "Note");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "cdf48a75-5dce-4bc7-a1cf-9272e71cecd6", new DateTime(2025, 4, 11, 3, 18, 45, 390, DateTimeKind.Local).AddTicks(5160), "AQAAAAIAAYagAAAAEGFKAm/7VEta+gtjIYTlYLvSvPoaBM0YNS0E4/rxqOfLoD9UrJEPHYwZj39H2dMuyQ==", "cc85f330-7bd7-4dfb-b956-d33bff8efe98" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.RenameColumn(
//                 name: "Note",
//                 table: "Curriculums",
//                 newName: "Not");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "51bb76a6-a4d3-409e-a93d-57e76c56b0fd", new DateTime(2025, 4, 10, 19, 12, 35, 402, DateTimeKind.Local).AddTicks(6693), "AQAAAAIAAYagAAAAEMRnnbi/jpnD5nu1ZdkE5PlQQgDpOJkRT2O7m0nG5lniNBJJefc8E6lleAMuXGeDVQ==", "2f0c8581-b780-4518-8d0d-fe275ef5a6c3" });
//         }
//     }
// }
