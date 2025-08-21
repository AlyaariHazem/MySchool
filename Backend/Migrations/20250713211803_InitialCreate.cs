using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dc07b579-b8a9-43f1-84f9-e69ca0e914d8", new DateTime(2025, 7, 14, 0, 18, 2, 915, DateTimeKind.Local).AddTicks(4386), "AQAAAAIAAYagAAAAEIzb/kTp9wunYX/3VgA/TREsiTWrFx44l12YPjM9ZBj1/3NEC0Viy6wV8BrZdPHOzQ==", "bc226b88-0337-4ad0-9b7f-4af186e88bb8" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "17eef585-6a2d-4602-8317-1805599dd7a1", new DateTime(2025, 5, 16, 4, 12, 22, 176, DateTimeKind.Local).AddTicks(5890), "AQAAAAIAAYagAAAAEChUnUXV7DaG2t48b5vPgay9s+ClOht0Kat59XASLhVUAZEi5uV46yTOAKWXkPsj0g==", "2110c7e5-89b8-459c-b384-52c23d86dc9a" });
        }
    }
}
