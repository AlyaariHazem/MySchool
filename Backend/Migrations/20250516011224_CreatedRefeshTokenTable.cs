using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CreatedRefeshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revoked = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "17eef585-6a2d-4602-8317-1805599dd7a1", new DateTime(2025, 5, 16, 4, 12, 22, 176, DateTimeKind.Local).AddTicks(5890), "AQAAAAIAAYagAAAAEChUnUXV7DaG2t48b5vPgay9s+ClOht0Kat59XASLhVUAZEi5uV46yTOAKWXkPsj0g==", "2110c7e5-89b8-459c-b384-52c23d86dc9a" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9a0decb7-494a-40bc-83e8-1e9090fec795", new DateTime(2025, 4, 28, 2, 9, 11, 181, DateTimeKind.Local).AddTicks(1806), "AQAAAAIAAYagAAAAEHLRtRF15E+b38K6SrkgSVzcpH/vPDaKCMaQt/tSFZfI6Cb2ao/WE+BA9tyYyLWxdQ==", "97f9350c-1252-4584-9c47-7c4ced3dadcd" });
        }
    }
}
