using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameToAlis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Accounts_AccountsAccountID",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_AccountsAccountID",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "AccountsAccountID",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "FullNameEng_MiddleNameEng",
                table: "Students",
                newName: "FullNameAlis_MiddleNameEng");

            migrationBuilder.RenameColumn(
                name: "FullNameEng_LastNameEng",
                table: "Students",
                newName: "FullNameAlis_LastNameEng");

            migrationBuilder.RenameColumn(
                name: "FullNameEng_FirstNameEng",
                table: "Students",
                newName: "FullNameAlis_FirstNameEng");

            migrationBuilder.AlterColumn<decimal>(
                name: "Receipt",
                table: "Vouchers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "HireDate",
                table: "Vouchers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<int>(
                name: "StudentID",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullNameAlis_MiddleNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullNameAlis_LastNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullNameAlis_FirstNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "FeeClass",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuardianID",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpenBalance",
                table: "Accounts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_AccountID",
                table: "Vouchers",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_StudentID",
                table: "Vouchers",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_GuardianID",
                table: "Attachments",
                column: "GuardianID");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_AccountID",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_StudentID",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_GuardianID",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "StudentID",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "FeeClass");

            migrationBuilder.DropColumn(
                name: "GuardianID",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "FullNameAlis_MiddleNameEng",
                table: "Students",
                newName: "FullNameEng_MiddleNameEng");

            migrationBuilder.RenameColumn(
                name: "FullNameAlis_LastNameEng",
                table: "Students",
                newName: "FullNameEng_LastNameEng");

            migrationBuilder.RenameColumn(
                name: "FullNameAlis_FirstNameEng",
                table: "Students",
                newName: "FullNameEng_FirstNameEng");

            migrationBuilder.AlterColumn<double>(
                name: "Receipt",
                table: "Vouchers",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "HireDate",
                table: "Vouchers",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "AccountsAccountID",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "FullNameEng_MiddleNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullNameEng_LastNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullNameEng_FirstNameEng",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "OpenBalance",
                table: "Accounts",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_AccountsAccountID",
                table: "Vouchers",
                column: "AccountsAccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Accounts_AccountsAccountID",
                table: "Vouchers",
                column: "AccountsAccountID",
                principalTable: "Accounts",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
