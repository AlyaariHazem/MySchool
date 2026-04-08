using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTenantsSubscriptionsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Vouchers_VoucherID",
                table: "Attachments");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxStudents = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantSettings",
                columns: table => new
                {
                    TenantSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSettings", x => x.TenantSettingsId);
                    table.ForeignKey(
                        name: "FK_TenantSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTenants",
                columns: table => new
                {
                    UserTenantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TenantRole = table.Column<int>(type: "int", nullable: false),
                    LastAccessedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTenants", x => x.UserTenantId);
                    table.ForeignKey(
                        name: "FK_UserTenants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTenants_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8469deea-476c-417f-9df1-93d69804b391", new DateTime(2026, 4, 8, 23, 37, 53, 376, DateTimeKind.Local).AddTicks(3259), "AQAAAAIAAYagAAAAEAmcG/oiZ7jZDk4rH/s+lr09UZkSx+VYa2Kh9Xr/+M7Wc0oRwaKhGKcpXrQ9S1E1bg==", "a99823cd-8c42-4adc-9e19-2160bed1359f" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_TenantId",
                table: "Subscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSettings_TenantId_Key",
                table: "TenantSettings",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_TenantId",
                table: "UserTenants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_UserId_TenantId",
                table: "UserTenants",
                columns: new[] { "UserId", "TenantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Vouchers_VoucherID",
                table: "Attachments",
                column: "VoucherID",
                principalTable: "Vouchers",
                principalColumn: "VoucherID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Vouchers_VoucherID",
                table: "Attachments");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "TenantSettings");

            migrationBuilder.DropTable(
                name: "UserTenants");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "44b8b47b-5c01-43b5-92b3-e3d93f3aaa9b", new DateTime(2025, 8, 21, 22, 39, 29, 351, DateTimeKind.Local).AddTicks(769), "AQAAAAIAAYagAAAAEIySt8apMWnHcsM3SL/YJr9Axk3YNI+DfCU0y+R6wzLqVcWAUC+LBpkYRFKcl53KcQ==", "3b1fb7f3-3cce-4d48-a85c-4d276ae22cab" });

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Vouchers_VoucherID",
                table: "Attachments",
                column: "VoucherID",
                principalTable: "Vouchers",
                principalColumn: "VoucherID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
