// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class deleteNoteFromAttachmentsTable : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_Attachments_vouchers_VoucherID",
//                 table: "Attachments");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_vouchers_AccountStudentGuardians_AccountStudentGuardianID",
//                 table: "vouchers");

//             migrationBuilder.DropPrimaryKey(
//                 name: "PK_vouchers",
//                 table: "vouchers");

//             migrationBuilder.DropColumn(
//                 name: "Note",
//                 table: "Attachments");

//             migrationBuilder.RenameTable(
//                 name: "vouchers",
//                 newName: "Vouchers");

//             migrationBuilder.RenameIndex(
//                 name: "IX_vouchers_AccountStudentGuardianID",
//                 table: "Vouchers",
//                 newName: "IX_Vouchers_AccountStudentGuardianID");

//             migrationBuilder.AddPrimaryKey(
//                 name: "PK_Vouchers",
//                 table: "Vouchers",
//                 column: "VoucherID");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "c3f10539-34b6-424f-b91b-cc4b0c2a062c", new DateTime(2025, 4, 7, 14, 58, 18, 895, DateTimeKind.Local).AddTicks(2589), "AQAAAAIAAYagAAAAEJg1EaF8bPck3j44vDiDpgsQvoV489doj5O9wPrd16tl21rAfrZ1l6PFh5jOWKvMFg==", "9ac277cb-0235-4d99-96a0-bd6da6d836c2" });

//             migrationBuilder.AddForeignKey(
//                 name: "FK_Attachments_Vouchers_VoucherID",
//                 table: "Attachments",
//                 column: "VoucherID",
//                 principalTable: "Vouchers",
//                 principalColumn: "VoucherID",
//                 onDelete: ReferentialAction.Cascade);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_Vouchers_AccountStudentGuardians_AccountStudentGuardianID",
//                 table: "Vouchers",
//                 column: "AccountStudentGuardianID",
//                 principalTable: "AccountStudentGuardians",
//                 principalColumn: "AccountStudentGuardianID",
//                 onDelete: ReferentialAction.Restrict);
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropForeignKey(
//                 name: "FK_Attachments_Vouchers_VoucherID",
//                 table: "Attachments");

//             migrationBuilder.DropForeignKey(
//                 name: "FK_Vouchers_AccountStudentGuardians_AccountStudentGuardianID",
//                 table: "Vouchers");

//             migrationBuilder.DropPrimaryKey(
//                 name: "PK_Vouchers",
//                 table: "Vouchers");

//             migrationBuilder.RenameTable(
//                 name: "Vouchers",
//                 newName: "vouchers");

//             migrationBuilder.RenameIndex(
//                 name: "IX_Vouchers_AccountStudentGuardianID",
//                 table: "vouchers",
//                 newName: "IX_vouchers_AccountStudentGuardianID");

//             migrationBuilder.AddColumn<string>(
//                 name: "Note",
//                 table: "Attachments",
//                 type: "nvarchar(max)",
//                 nullable: true);

//             migrationBuilder.AddPrimaryKey(
//                 name: "PK_vouchers",
//                 table: "vouchers",
//                 column: "VoucherID");

//             migrationBuilder.UpdateData(
//                 table: "AspNetUsers",
//                 keyColumn: "Id",
//                 keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
//                 columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
//                 values: new object[] { "8f2b0842-d86f-4fad-8c0a-39bb750a812c", new DateTime(2025, 4, 6, 23, 45, 14, 806, DateTimeKind.Local).AddTicks(421), "AQAAAAIAAYagAAAAEHYOZUMgZjOYMnIerB4VPsfvP78Zz4oAX9ivRDSsxVsLwLtuNVdHYQ2GSejRWgBzHg==", "5a57fa05-d7a0-402c-a450-e54d61afaaf5" });

//             migrationBuilder.AddForeignKey(
//                 name: "FK_Attachments_vouchers_VoucherID",
//                 table: "Attachments",
//                 column: "VoucherID",
//                 principalTable: "vouchers",
//                 principalColumn: "VoucherID",
//                 onDelete: ReferentialAction.Cascade);

//             migrationBuilder.AddForeignKey(
//                 name: "FK_vouchers_AccountStudentGuardians_AccountStudentGuardianID",
//                 table: "vouchers",
//                 column: "AccountStudentGuardianID",
//                 principalTable: "AccountStudentGuardians",
//                 principalColumn: "AccountStudentGuardianID",
//                 onDelete: ReferentialAction.Restrict);
//         }
//     }
// }
