using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "44b8b47b-5c01-43b5-92b3-e3d93f3aaa9b", new DateTime(2025, 8, 21, 22, 39, 29, 351, DateTimeKind.Local).AddTicks(769), "AQAAAAIAAYagAAAAEIySt8apMWnHcsM3SL/YJr9Axk3YNI+DfCU0y+R6wzLqVcWAUC+LBpkYRFKcl53KcQ==", "3b1fb7f3-3cce-4d48-a85c-4d276ae22cab" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dc07b579-b8a9-43f1-84f9-e69ca0e914d8", new DateTime(2025, 7, 14, 0, 18, 2, 915, DateTimeKind.Local).AddTicks(4386), "AQAAAAIAAYagAAAAEIzb/kTp9wunYX/3VgA/TREsiTWrFx44l12YPjM9ZBj1/3NEC0Viy6wV8BrZdPHOzQ==", "bc226b88-0337-4ad0-9b7f-4af186e88bb8" });
        }
    }
}
