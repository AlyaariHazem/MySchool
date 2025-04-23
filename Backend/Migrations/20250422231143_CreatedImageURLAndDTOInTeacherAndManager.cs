using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CreatedImageURLAndDTOInTeacherAndManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DOB",
                table: "Teachers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DOB",
                table: "Managers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Managers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8c318b89-bbf2-4a4c-ad1a-0df165a35bd7", new DateTime(2025, 4, 23, 2, 11, 41, 799, DateTimeKind.Local).AddTicks(1825), "AQAAAAIAAYagAAAAEBiaPI29rX8WaR3BbClF4RY2Ys7vE4rHejGjPrCNYFEs0ajpqRbznKj2it1YxR98+A==", "31577761-af89-4cb1-a36c-c5bb84d58ac0" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DOB",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "DOB",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Managers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0532ad00-74be-4463-a40b-aa67d358cdbc", new DateTime(2025, 4, 22, 3, 59, 39, 410, DateTimeKind.Local).AddTicks(1788), "AQAAAAIAAYagAAAAEEAIjqonqPZ25iNFUwHNzM8uYj4dKCmNw5C4AXgiFCqvYXf4T5iJWgeOpIYK8YVwVA==", "c4cc4a64-3c32-4c1c-8371-b5827d599ebd" });
        }
    }
}
