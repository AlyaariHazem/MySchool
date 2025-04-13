// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class addManagerIdTOTenant2 : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_Tenants_Managers_ManagerId",
//                 table: "Tenants");

//             migrationBuilder.DropIndex(
//                 name: "IX_Tenants_ManagerId",
//                 table: "Tenants");

//             migrationBuilder.DropColumn(
//                 name: "ManagerId",
//                 table: "Tenants");

//             migrationBuilder.AddColumn<int>(
//                 name: "TenantID",
//                 table: "Managers",
//                 type: "int",
//                 nullable: true);

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "0010f0ca-e668-4476-b27a-bf7c7697ae3f", new DateTime(2025, 2, 25, 1, 48, 42, 646, DateTimeKind.Local).AddTicks(288), "AQAAAAIAAYagAAAAELjblt8fvZmXkHxejXPokcOZZDoSmezV4kOQRSBvq0rtOhepxZdr153rv6Z40ZTPTA==", "81b7eab6-0bc7-47da-b3e8-a156f583304c" });

//             migrationBuilder.CreateIndex(
//                 name: "IX_Managers_TenantID",
//                 table: "Managers",
//                 column: "TenantID",
//                 unique: true,
//                 filter: "[TenantID] IS NOT NULL");

//             migrationBuilder.AddForeignKey(
//                 name: "FK_Managers_Tenants_TenantID",
//                 table: "Managers",
//                 column: "TenantID",
//                 principalTable: "Tenants",
//                 principalColumn: "TenantId",
//                 onDelete: ReferentialAction.Restrict);
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_Managers_Tenants_TenantID",
//                 table: "Managers");

//             migrationBuilder.DropIndex(
//                 name: "IX_Managers_TenantID",
//                 table: "Managers");

//             migrationBuilder.DropColumn(
//                 name: "TenantID",
//                 table: "Managers");

//             migrationBuilder.AddColumn<int>(
//                 name: "ManagerId",
//                 table: "Tenants",
//                 type: "int",
//                 nullable: true);

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "5a4b2f9a-f0fa-4df2-a4dd-c03e3465b406", new DateTime(2025, 2, 24, 23, 44, 43, 217, DateTimeKind.Local).AddTicks(6703), "AQAAAAIAAYagAAAAEIwhbFjo5EeS/DakzE0Gqsk3SMboFgF/z5NZZ8La7VpXz47M5sst7v4+uAtkjuvYfw==", "5792b4c5-2573-414a-ac2b-bc10b455a094" });

//             migrationBuilder.CreateIndex(
//                 name: "IX_Tenants_ManagerId",
//                 table: "Tenants",
//                 column: "ManagerId",
//                 unique: true,
//                 filter: "[ManagerId] IS NOT NULL");

//             migrationBuilder.AddForeignKey(
//                 name: "FK_Tenants_Managers_ManagerId",
//                 table: "Tenants",
//                 column: "ManagerId",
//                 principalTable: "Managers",
//                 principalColumn: "ManagerID",
//                 onDelete: ReferentialAction.Restrict);
//         }
//     }
// }
