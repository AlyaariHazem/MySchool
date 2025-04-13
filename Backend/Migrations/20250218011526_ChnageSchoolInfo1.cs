// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class ChnageSchoolInfo1 : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.AddColumn<string>(
//                 name: "SchoolCategory",
//                 table: "Schools",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "3234dee8-c4e3-44a9-9738-dae488693024", new DateTime(2025, 2, 18, 4, 15, 25, 569, DateTimeKind.Local).AddTicks(5460), "AQAAAAIAAYagAAAAEP1Nsl6vZguEHHerOwjlaC1ke31ErGzvrKXfBTAHbYpJ2GqDunv3QnkO/pHPROUacw==", "4afa8e8f-10fb-4aca-8d83-702de6e4bff4" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropColumn(
//                 name: "SchoolCategory",
//                 table: "Schools");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "11adceea-8e3d-409d-826c-3a98ae048a00", new DateTime(2025, 2, 18, 4, 4, 28, 405, DateTimeKind.Local).AddTicks(8144), "AQAAAAIAAYagAAAAEGrtMDEU/KrWj+HMStfwxOGiP0WVGK56f3CXjgzEBtEV31J7Hp8UswQyz5FDysondw==", "fbb80392-1e95-4073-b056-8bd2c5bcaadf" });
//         }
//     }
// }
