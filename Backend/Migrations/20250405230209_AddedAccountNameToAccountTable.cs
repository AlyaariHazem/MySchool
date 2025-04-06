using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedAccountNameToAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bcd840b2-5a2f-40b7-b6da-10732055abd1", new DateTime(2025, 4, 6, 2, 2, 3, 227, DateTimeKind.Local).AddTicks(9863), "AQAAAAIAAYagAAAAENad8ATpM2IotWlRXxWdDID8BrOKrQJVlEp0Zq8cRjdCEQTtrea1L0GDhG2vS2V7rA==", "5dbf107d-16ab-4cfb-a149-11af021e5bf3" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "Accounts");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ea234470-4dc7-42f0-9003-e2177fab6129", new DateTime(2025, 4, 5, 23, 0, 1, 789, DateTimeKind.Local).AddTicks(3951), "AQAAAAIAAYagAAAAECKT88mV4k1rFR45rIvJG3IJsEUXyqemHHAkBsvqchvFy0L2H8BgizPqjnIl96+QOw==", "d9bbcb4d-2b81-422f-bc20-ab140ec3cf9f" });
        }
    }
}
