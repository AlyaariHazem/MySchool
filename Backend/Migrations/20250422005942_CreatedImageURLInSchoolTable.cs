using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CreatedImageURLInSchoolTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0532ad00-74be-4463-a40b-aa67d358cdbc", new DateTime(2025, 4, 22, 3, 59, 39, 410, DateTimeKind.Local).AddTicks(1788), "AQAAAAIAAYagAAAAEEAIjqonqPZ25iNFUwHNzM8uYj4dKCmNw5C4AXgiFCqvYXf4T5iJWgeOpIYK8YVwVA==", "c4cc4a64-3c32-4c1c-8371-b5827d599ebd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Schools");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "da925df1-2f04-4c91-a39e-d8c15879a490", new DateTime(2025, 4, 18, 22, 5, 20, 363, DateTimeKind.Local).AddTicks(1516), "AQAAAAIAAYagAAAAEHUfqMWqU5V/GQ9c0QHPVwfiz82avdTMOxcUdcBX37+GzAeUe5ZSbBAUXnox8tWMRQ==", "ed725e68-f34c-417e-9cfd-5a3c2131df45" });
        }
    }
}
