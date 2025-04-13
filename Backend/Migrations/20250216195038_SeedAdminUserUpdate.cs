// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class SeedAdminUserUpdate : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "Address", "ConcurrencyStamp", "EmailConfirmed", "Gender", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { null, "62cb14d6-ffe6-42ba-a0e7-62045a200dc7", true, "", new DateTime(2025, 2, 16, 22, 50, 37, 411, DateTimeKind.Local).AddTicks(8545), "AQAAAAIAAYagAAAAEBZPw6lVQHEPASQU1eWFbhSjrEH814XGB0YTWleOFSJqWHgZd+HsV/lfBEqOpGzd9A==", "4f9d1606-83c3-4e1c-9376-653567142c73" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "Address", "ConcurrencyStamp", "EmailConfirmed", "Gender", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "Sana'a", "827625a2-391e-4e76-be03-680fa619f39e", false, "Male", new DateTime(2025, 2, 16, 22, 30, 24, 345, DateTimeKind.Local).AddTicks(3010), "AQAAAAIAAYagAAAAEP5kxR11x0YSGyX6IA2PUaut972i6iWAmRolXmoZVhqnK2g0u+HkKK8VF7+nPNWkZA==", "b65d9234-dfe7-4027-98c3-c3f2e2fe3fb9" });
//         }
//     }
// }
