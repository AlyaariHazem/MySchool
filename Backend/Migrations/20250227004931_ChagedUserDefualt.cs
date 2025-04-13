// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class ChagedUserDefualt : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "Email", "HireDate", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName", "UserType" },
//                 values: new object[] { "683c84d0-24e6-48af-81dd-8892fd24a1d4", "ALYAARIHAZEM@GMAIL.COM", new DateTime(2025, 2, 27, 3, 49, 30, 203, DateTimeKind.Local).AddTicks(5919), "ALYAARIHAZEM@GMAIL.COM", "MANAGER", "AQAAAAIAAYagAAAAEDzYYajqNuv+djJbS7f8NUKKgbhXCeQe0md5BvhouU0V2/fxituNusXPDCdG1moTzA==", "40fd6df1-ef73-45ca-927c-65095b5a410e", "MANAGER", "MANAGER" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "Email", "HireDate", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName", "UserType" },
//                 values: new object[] { "0010f0ca-e668-4476-b27a-bf7c7697ae3f", "admin@gmail.com", new DateTime(2025, 2, 25, 1, 48, 42, 646, DateTimeKind.Local).AddTicks(288), "ADMIN@GMAIL.COM", "ADMIN", "AQAAAAIAAYagAAAAELjblt8fvZmXkHxejXPokcOZZDoSmezV4kOQRSBvq0rtOhepxZdr153rv6Z40ZTPTA==", "81b7eab6-0bc7-47da-b3e8-a156f583304c", "Admin", "Admin" });
//         }
//     }
// }
