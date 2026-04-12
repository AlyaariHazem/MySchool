using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RegistrationRequestPhoneAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistrationRequests_NormalizedEmail",
                table: "RegistrationRequests");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "RegistrationRequests",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RegistrationRequests",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "RegistrationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "RegistrationRequests",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedPhone",
                table: "RegistrationRequests",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "RegistrationRequests",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumberNormalized",
                table: "AspNetUsers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            // Old rows had no phone; empty NormalizedPhone would violate the new unique index.
            migrationBuilder.Sql("DELETE FROM RegistrationRequests;");

            migrationBuilder.CreateTable(
                name: "RegistrationRequestAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationRequestId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationRequestAttachments_RegistrationRequests_RegistrationRequestId",
                        column: x => x.RegistrationRequestId,
                        principalTable: "RegistrationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_NormalizedPhone",
                table: "RegistrationRequests",
                column: "NormalizedPhone",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers",
                column: "PhoneNumberNormalized",
                unique: true,
                filter: "[PhoneNumberNormalized] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequestAttachments_RegistrationRequestId",
                table: "RegistrationRequestAttachments",
                column: "RegistrationRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationRequestAttachments");

            migrationBuilder.DropIndex(
                name: "IX_RegistrationRequests_NormalizedPhone",
                table: "RegistrationRequests");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "NormalizedPhone",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumberNormalized",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "RegistrationRequests",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RegistrationRequests",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "HireDate", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 8, 21, 0, 57, 735, DateTimeKind.Utc).AddTicks(6187), "AQAAAAIAAYagAAAAEIauMaht3feaQi/+ngEZRiZXg+qA2bn7z77pRg1JMoNUCOspaR3/rJtVTvnkdj0VDw==" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationRequests_NormalizedEmail",
                table: "RegistrationRequests",
                column: "NormalizedEmail",
                unique: true,
                filter: "[Status] = 0");
        }
    }
}
