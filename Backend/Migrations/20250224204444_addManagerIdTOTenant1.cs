// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class addManagerIdTOTenant1 : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "5a4b2f9a-f0fa-4df2-a4dd-c03e3465b406", new DateTime(2025, 2, 24, 23, 44, 43, 217, DateTimeKind.Local).AddTicks(6703), "AQAAAAIAAYagAAAAEIwhbFjo5EeS/DakzE0Gqsk3SMboFgF/z5NZZ8La7VpXz47M5sst7v4+uAtkjuvYfw==", "5792b4c5-2573-414a-ac2b-bc10b455a094" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "bedf0368-927a-4b68-96f7-2dd27b4fa6a3", new DateTime(2025, 2, 24, 23, 37, 2, 177, DateTimeKind.Local).AddTicks(5970), "AQAAAAIAAYagAAAAEMe94oJi7E1DeSPGLVvUnlSxMRk6SLF16KE0zVs1FBoiLQzcOQE2BOFx6sFcsVvXUA==", "d98c7782-2ac9-40ef-adda-5f2330458967" });
//         }
//     }
// }
