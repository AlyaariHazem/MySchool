// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// #pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class addManagerIdTOTenant : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.AddColumn<int>(
//                 name: "ManagerId",
//                 table: "Tenants",
//                 type: "int",
//                 nullable: true);

//             migrationBuilder.UpdateData(
//                 table: "AspNetRoles",
//                 keyColumn: "Id",
//                 keyValue: "1",
//                 columns: new[] { "Name", "NormalizedName" },
//                 values: new object[] { "MANAGER", "MANAGER" });

//             migrationBuilder.InsertData(
//                 table: "AspNetRoles",
//                 columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
//                 values: new object[,]
//                 {
//                     { "2", null, "STUDENT", "STUDENT" },
//                     { "3", null, "TEACHER", "TEACHER" },
//                     { "4", null, "GUARDIAN", "GUARDIAN" }
//                 });

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "bedf0368-927a-4b68-96f7-2dd27b4fa6a3", new DateTime(2025, 2, 24, 23, 37, 2, 177, DateTimeKind.Local).AddTicks(5970), "AQAAAAIAAYagAAAAEMe94oJi7E1DeSPGLVvUnlSxMRk6SLF16KE0zVs1FBoiLQzcOQE2BOFx6sFcsVvXUA==", "d98c7782-2ac9-40ef-adda-5f2330458967" });

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

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_Tenants_Managers_ManagerId",
//                 table: "Tenants");

//             migrationBuilder.DropIndex(
//                 name: "IX_Tenants_ManagerId",
//                 table: "Tenants");

//             migrationBuilder.DeleteData(
//                 table: "AspNetRoles",
//                 keyColumn: "Id",
//                 keyValue: "2");

//             migrationBuilder.DeleteData(
//                 table: "AspNetRoles",
//                 keyColumn: "Id",
//                 keyValue: "3");

//             migrationBuilder.DeleteData(
//                 table: "AspNetRoles",
//                 keyColumn: "Id",
//                 keyValue: "4");

//             migrationBuilder.DropColumn(
//                 name: "ManagerId",
//                 table: "Tenants");

//             migrationBuilder.UpdateData(
//                 table: "AspNetRoles",
//                 keyColumn: "Id",
//                 keyValue: "1",
//                 columns: new[] { "Name", "NormalizedName" },
//                 values: new object[] { "Admin", "ADMIN" });

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "17cfa2a0-1505-44f7-b6ff-1204c69bed6d", new DateTime(2025, 2, 23, 23, 55, 28, 471, DateTimeKind.Local).AddTicks(6878), "AQAAAAIAAYagAAAAELq/4QSE/+sDl1eIVvZGWDhHQQ/WiNbNq37NjQ2pmJwQxfHUJHsx3a/BMqjHl0dauw==", "a497c0a8-2d9d-4efe-9693-014ee00d2891" });
//         }
//     }
// }
