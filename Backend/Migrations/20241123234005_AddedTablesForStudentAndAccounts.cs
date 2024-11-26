using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedTablesForStudentAndAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Guardians_GuardianID",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Accounts_AccountID",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Students_StudentID",
                table: "Vouchers");

            migrationBuilder.DropTable(
                name: "StudentClass");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vouchers",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Fee",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "School_Crea_Date",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "NoteDiscount",
                table: "Fees");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "FeeClass");

            migrationBuilder.DropColumn(
                name: "PlaceBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Accounts");

            migrationBuilder.RenameTable(
                name: "Vouchers",
                newName: "vouchers");

            migrationBuilder.RenameIndex(
                name: "IX_Vouchers_StudentID",
                table: "vouchers",
                newName: "IX_vouchers_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_Vouchers_AccountID",
                table: "vouchers",
                newName: "IX_vouchers_AccountID");

            migrationBuilder.RenameColumn(
                name: "discount",
                table: "Fees",
                newName: "FeeNameAlis");

            migrationBuilder.RenameColumn(
                name: "GuardianID",
                table: "Attachments",
                newName: "VoucherID");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_GuardianID",
                table: "Attachments",
                newName: "IX_Attachments_VoucherID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "YearDateStart",
                table: "Years",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "YearDateEnd",
                table: "Years",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "HireDate",
                table: "Years",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "PlaceBirth",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Schools",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SalaryMonth",
                table: "Salarys",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SalaryHireDate",
                table: "Salarys",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<bool>(
                name: "Mandatory",
                table: "FeeClass",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Attachments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpenBalance",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "HireDate",
                table: "Accounts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentID",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeAccountID",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_vouchers",
                table: "vouchers",
                column: "VoucherID");

            migrationBuilder.CreateTable(
                name: "StudentClassFees",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    FeeID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    AmountDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NoteDiscount = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClassFees", x => new { x.ClassID, x.FeeID });
                    table.ForeignKey(
                        name: "FK_StudentClassFees_FeeClass_ClassID_FeeID",
                        columns: x => new { x.ClassID, x.FeeID },
                        principalTable: "FeeClass",
                        principalColumns: new[] { "FeeID", "ClassID" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentClassFees_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TypeAccounts",
                columns: table => new
                {
                    TypeAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeAccountName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeAccounts", x => x.TypeAccountID);
                });

            migrationBuilder.InsertData(
                table: "TypeAccounts",
                columns: new[] { "TypeAccountID", "TypeAccountName" },
                values: new object[,]
                {
                    { 1, "Guardain" },
                    { 2, "School" },
                    { 3, "Branches" },
                    { 4, "Funds" },
                    { 5, "Employees" },
                    { 6, "Banks" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_StudentID",
                table: "Accounts",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TypeAccountID",
                table: "Accounts",
                column: "TypeAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassFees_StudentID",
                table: "StudentClassFees",
                column: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Students_StudentID",
                table: "Accounts",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_TypeAccounts_TypeAccountID",
                table: "Accounts",
                column: "TypeAccountID",
                principalTable: "TypeAccounts",
                principalColumn: "TypeAccountID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_vouchers_VoucherID",
                table: "Attachments",
                column: "VoucherID",
                principalTable: "vouchers",
                principalColumn: "VoucherID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_vouchers_Accounts_AccountID",
                table: "vouchers",
                column: "AccountID",
                principalTable: "Accounts",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_vouchers_Students_StudentID",
                table: "vouchers",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Students_StudentID",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_TypeAccounts_TypeAccountID",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_vouchers_VoucherID",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_vouchers_Accounts_AccountID",
                table: "vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_vouchers_Students_StudentID",
                table: "vouchers");

            migrationBuilder.DropTable(
                name: "StudentClassFees");

            migrationBuilder.DropTable(
                name: "TypeAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vouchers",
                table: "vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_StudentID",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_TypeAccountID",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PlaceBirth",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Mandatory",
                table: "FeeClass");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "StudentID",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TypeAccountID",
                table: "Accounts");

            migrationBuilder.RenameTable(
                name: "vouchers",
                newName: "Vouchers");

            migrationBuilder.RenameIndex(
                name: "IX_vouchers_StudentID",
                table: "Vouchers",
                newName: "IX_Vouchers_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_vouchers_AccountID",
                table: "Vouchers",
                newName: "IX_Vouchers_AccountID");

            migrationBuilder.RenameColumn(
                name: "FeeNameAlis",
                table: "Fees",
                newName: "discount");

            migrationBuilder.RenameColumn(
                name: "VoucherID",
                table: "Attachments",
                newName: "GuardianID");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_VoucherID",
                table: "Attachments",
                newName: "IX_Attachments_GuardianID");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "YearDateStart",
                table: "Years",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "YearDateEnd",
                table: "Years",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "HireDate",
                table: "Years",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "Fee",
                table: "Students",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "School_Crea_Date",
                table: "Schools",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<DateOnly>(
                name: "SalaryMonth",
                table: "Salarys",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "SalaryHireDate",
                table: "Salarys",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "NoteDiscount",
                table: "Fees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "FeeClass",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceBirth",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpenBalance",
                table: "Accounts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "HireDate",
                table: "Accounts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vouchers",
                table: "Vouchers",
                column: "VoucherID");

            migrationBuilder.CreateTable(
                name: "StudentClass",
                columns: table => new
                {
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClass", x => new { x.StudentID, x.ClassID });
                    table.ForeignKey(
                        name: "FK_StudentClass_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentClass_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentClass_ClassID",
                table: "StudentClass",
                column: "ClassID");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Guardians_GuardianID",
                table: "Attachments",
                column: "GuardianID",
                principalTable: "Guardians",
                principalColumn: "GuardianID");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Accounts_AccountID",
                table: "Vouchers",
                column: "AccountID",
                principalTable: "Accounts",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Students_StudentID",
                table: "Vouchers",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
