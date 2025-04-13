// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class addedMandatoryToStudentClassFees : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.AddColumn<bool>(
//                 name: "Mandatory",
//                 table: "StudentClassFees",
//                 type: "bit",
//                 nullable: true);

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "8f2b0842-d86f-4fad-8c0a-39bb750a812c", new DateTime(2025, 4, 6, 23, 45, 14, 806, DateTimeKind.Local).AddTicks(421), "AQAAAAIAAYagAAAAEHYOZUMgZjOYMnIerB4VPsfvP78Zz4oAX9ivRDSsxVsLwLtuNVdHYQ2GSejRWgBzHg==", "5a57fa05-d7a0-402c-a450-e54d61afaaf5" });
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropColumn(
//                 name: "Mandatory",
//                 table: "StudentClassFees");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "bcd840b2-5a2f-40b7-b6da-10732055abd1", new DateTime(2025, 4, 6, 2, 2, 3, 227, DateTimeKind.Local).AddTicks(9863), "AQAAAAIAAYagAAAAENad8ATpM2IotWlRXxWdDID8BrOKrQJVlEp0Zq8cRjdCEQTtrea1L0GDhG2vS2V7rA==", "5dbf107d-16ab-4cfb-a149-11af021e5bf3" });
//         }
//     }
// }
