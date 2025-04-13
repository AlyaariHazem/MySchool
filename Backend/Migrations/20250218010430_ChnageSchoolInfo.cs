// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class ChnageSchoolInfo : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.AddColumn<string>(
//                 name: "Address",
//                 table: "Schools",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.AddColumn<string>(
//                 name: "Description",
//                 table: "Schools",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.AddColumn<string>(
//                 name: "Mobile",
//                 table: "Schools",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.AddColumn<string>(
//                 name: "Website",
//                 table: "Schools",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "11adceea-8e3d-409d-826c-3a98ae048a00", new DateTime(2025, 2, 18, 4, 4, 28, 405, DateTimeKind.Local).AddTicks(8144), "AQAAAAIAAYagAAAAEGrtMDEU/KrWj+HMStfwxOGiP0WVGK56f3CXjgzEBtEV31J7Hp8UswQyz5FDysondw==", "fbb80392-1e95-4073-b056-8bd2c5bcaadf" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropColumn(
//                 name: "Address",
//                 table: "Schools");

//             migrationBuilder.DropColumn(
//                 name: "Description",
//                 table: "Schools");

//             migrationBuilder.DropColumn(
//                 name: "Mobile",
//                 table: "Schools");

//             migrationBuilder.DropColumn(
//                 name: "Website",
//                 table: "Schools");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "62cb14d6-ffe6-42ba-a0e7-62045a200dc7", new DateTime(2025, 2, 16, 22, 50, 37, 411, DateTimeKind.Local).AddTicks(8545), "AQAAAAIAAYagAAAAEBZPw6lVQHEPASQU1eWFbhSjrEH814XGB0YTWleOFSJqWHgZd+HsV/lfBEqOpGzd9A==", "4f9d1606-83c3-4e1c-9376-653567142c73" });
//         }
//     }
// }
