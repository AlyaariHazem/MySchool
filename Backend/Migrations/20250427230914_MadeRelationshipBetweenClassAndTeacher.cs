using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class MadeRelationshipBetweenClassAndTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeacherID",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9a0decb7-494a-40bc-83e8-1e9090fec795", new DateTime(2025, 4, 28, 2, 9, 11, 181, DateTimeKind.Local).AddTicks(1806), "AQAAAAIAAYagAAAAEHLRtRF15E+b38K6SrkgSVzcpH/vPDaKCMaQt/tSFZfI6Cb2ao/WE+BA9tyYyLWxdQ==", "97f9350c-1252-4584-9c47-7c4ced3dadcd" });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherID",
                table: "Classes",
                column: "TeacherID");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Teachers_TeacherID",
                table: "Classes",
                column: "TeacherID",
                principalTable: "Teachers",
                principalColumn: "TeacherID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Teachers_TeacherID",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_TeacherID",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "TeacherID",
                table: "Classes");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8c318b89-bbf2-4a4c-ad1a-0df165a35bd7", new DateTime(2025, 4, 23, 2, 11, 41, 799, DateTimeKind.Local).AddTicks(1825), "AQAAAAIAAYagAAAAEBiaPI29rX8WaR3BbClF4RY2Ys7vE4rHejGjPrCNYFEs0ajpqRbznKj2it1YxR98+A==", "31577761-af89-4cb1-a36c-c5bb84d58ac0" });
        }
    }
}
