using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedFeesAccountsAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "FullName_FirstName",
                table: "Guardians");

            migrationBuilder.DropColumn(
                name: "FullName_LastName",
                table: "Guardians");

            migrationBuilder.RenameColumn(
                name: "Job",
                table: "Guardians",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "FullName_MiddleName",
                table: "Guardians",
                newName: "FullName");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Students",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceBirth",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Guardians",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpenBalance = table.Column<double>(type: "float", nullable: false),
                    TypeOpenBalance = table.Column<bool>(type: "bit", nullable: false),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GuardianID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_Accounts_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AttachmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttachmentURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentID);
                    table.ForeignKey(
                        name: "FK_Attachments_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fees",
                columns: table => new
                {
                    FeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    discount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoteDiscount = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fees", x => x.FeeID);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    VoucherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Receipt = table.Column<double>(type: "float", nullable: false),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    AccountsAccountID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.VoucherID);
                    table.ForeignKey(
                        name: "FK_Vouchers_Accounts_AccountsAccountID",
                        column: x => x.AccountsAccountID,
                        principalTable: "Accounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeClass",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    FeeID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeClass", x => new { x.FeeID, x.ClassID });
                    table.ForeignKey(
                        name: "FK_FeeClass_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeClass_Fees_FeeID",
                        column: x => x.FeeID,
                        principalTable: "Fees",
                        principalColumn: "FeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Guardians",
                columns: new[] { "GuardianID", "Address", "DateOfBirth", "Email", "FullName", "Phone", "TypeGuardian" },
                values: new object[,]
                {
                    { 1, null, null, null, "School", null, null },
                    { 2, null, null, null, "Branches", null, null },
                    { 3, null, null, null, "Fuands", null, null },
                    { 4, null, null, null, "Guardians", null, null },
                    { 5, null, null, null, "Employees", null, null },
                    { 6, null, null, null, "Bacnks", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_GuardianID",
                table: "Accounts",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_StudentID",
                table: "Attachments",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_FeeClass_ClassID",
                table: "FeeClass",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_AccountsAccountID",
                table: "Vouchers",
                column: "AccountsAccountID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "FeeClass");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "Fees");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Guardians",
                keyColumn: "GuardianID",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PlaceBirth",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Guardians");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Guardians",
                newName: "FullName_MiddleName");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Guardians",
                newName: "Job");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Guardians",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName_FirstName",
                table: "Guardians",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName_LastName",
                table: "Guardians",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
