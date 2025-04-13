// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class AddSomethingToStudent : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.CreateTable(
//                 name: "Tenants",
//                 columns: table => new
//                 {
//                     TenantId = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Tenants", x => x.TenantId);
//                 });

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "17cfa2a0-1505-44f7-b6ff-1204c69bed6d", new DateTime(2025, 2, 23, 23, 55, 28, 471, DateTimeKind.Local).AddTicks(6878), "AQAAAAIAAYagAAAAELq/4QSE/+sDl1eIVvZGWDhHQQ/WiNbNq37NjQ2pmJwQxfHUJHsx3a/BMqjHl0dauw==", "a497c0a8-2d9d-4efe-9693-014ee00d2891" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropTable(
//                 name: "Tenants");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "3234dee8-c4e3-44a9-9738-dae488693024", new DateTime(2025, 2, 18, 4, 15, 25, 569, DateTimeKind.Local).AddTicks(5460), "AQAAAAIAAYagAAAAEP1Nsl6vZguEHHerOwjlaC1ke31ErGzvrKXfBTAHbYpJ2GqDunv3QnkO/pHPROUacw==", "4afa8e8f-10fb-4aca-8d83-702de6e4bff4" });
//         }
//     }
// }
