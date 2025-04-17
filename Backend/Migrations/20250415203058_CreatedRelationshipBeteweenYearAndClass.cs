using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CreatedRelationshipBeteweenYearAndClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YearID",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "923d87ae-4ad0-4bc7-ba21-cdba47dbc9c7", new DateTime(2025, 4, 15, 23, 30, 56, 613, DateTimeKind.Local).AddTicks(1854), "AQAAAAIAAYagAAAAEH092gzxaA6EXhSvxR8nSVswHeYNxQV0cY9uXWrBttxn5lNxkZIvAODSTFVw7/OIDA==", "eaa23200-1614-4613-bbf9-33fa8d691576" });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_YearID",
                table: "Classes",
                column: "YearID");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Years_YearID",
                table: "Classes",
                column: "YearID",
                principalTable: "Years",
                principalColumn: "YearID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Years_YearID",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_YearID",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "YearID",
                table: "Classes");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "658000ad-682a-4611-9071-b5123adfcefc", new DateTime(2025, 4, 15, 21, 43, 5, 137, DateTimeKind.Local).AddTicks(5713), "AQAAAAIAAYagAAAAEBzf3iHsDiNTk8adjuEGxpJLnoGtJJgrZlUEQ7W1lwF4KFWsjzKVzITUaE1caEQzJw==", "12570dc5-4457-4b1a-ae73-f7f3fe9fd8eb" });
        }
    }
}
