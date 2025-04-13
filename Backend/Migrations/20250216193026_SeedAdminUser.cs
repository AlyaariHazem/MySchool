// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class SeedAdminUser : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.InsertData(
//                 table: "AspNetRoles",
//                 columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
//                 values: new object[] { "1", null, "Admin", "ADMIN" });

//             migrationBuilder.InsertData(
//                 table: "AspNetUsers",
//                 columns: new[] { "Id", "AccessFailedCount", "Address", "ConcurrencyStamp", "Email", "EmailConfirmed", "Gender", "HireDate", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName", "UserType" },
//                 values: new object[] { "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec", 0, "Sana'a", "827625a2-391e-4e76-be03-680fa619f39e", "admin@gmail.com", false, "Male", new DateTime(2025, 2, 16, 22, 30, 24, 345, DateTimeKind.Local).AddTicks(3010), false, null, "ADMIN@GMAIL.COM", "ADMIN", "AQAAAAIAAYagAAAAEP5kxR11x0YSGyX6IA2PUaut972i6iWAmRolXmoZVhqnK2g0u+HkKK8VF7+nPNWkZA==", null, false, "b65d9234-dfe7-4027-98c3-c3f2e2fe3fb9", false, "Admin", "Admin" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DeleteData(
//                 table: "AspNetRoles",
//                 keyColumn: "Id",
//                 keyValue: "1");

//             migrationBuilder.DeleteData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec");
//         }
//     }
// }
