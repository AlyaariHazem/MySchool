using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationshipsBetweenStudentGuardianAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Guardians_GuardianID",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Students_StudentID",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_vouchers_Accounts_AccountID",
                table: "vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_vouchers_Students_StudentID",
                table: "vouchers");

            migrationBuilder.DropIndex(
                name: "IX_vouchers_StudentID",
                table: "vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_GuardianID",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_StudentID",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "StudentID",
                table: "vouchers");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "GuardianID",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "StudentID",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "AccountID",
                table: "vouchers",
                newName: "AccountStudentGuardianID");

            migrationBuilder.RenameIndex(
                name: "IX_vouchers_AccountID",
                table: "vouchers",
                newName: "IX_vouchers_AccountStudentGuardianID");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "vouchers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "GuardianID",
                table: "Students",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "AccountStudentGuardians",
                columns: table => new
                {
                    AccountStudentGuardianID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    GuardianID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountStudentGuardians", x => x.AccountStudentGuardianID);
                    table.ForeignKey(
                        name: "FK_AccountStudentGuardians_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountStudentGuardians_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountStudentGuardians_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountStudentGuardians_AccountID",
                table: "AccountStudentGuardians",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountStudentGuardians_GuardianID",
                table: "AccountStudentGuardians",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountStudentGuardians_StudentID",
                table: "AccountStudentGuardians",
                column: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students",
                column: "GuardianID",
                principalTable: "Guardians",
                principalColumn: "GuardianID");

            migrationBuilder.AddForeignKey(
                name: "FK_vouchers_AccountStudentGuardians_AccountStudentGuardianID",
                table: "vouchers",
                column: "AccountStudentGuardianID",
                principalTable: "AccountStudentGuardians",
                principalColumn: "AccountStudentGuardianID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_vouchers_AccountStudentGuardians_AccountStudentGuardianID",
                table: "vouchers");

            migrationBuilder.DropTable(
                name: "AccountStudentGuardians");

            migrationBuilder.RenameColumn(
                name: "AccountStudentGuardianID",
                table: "vouchers",
                newName: "AccountID");

            migrationBuilder.RenameIndex(
                name: "IX_vouchers_AccountStudentGuardianID",
                table: "vouchers",
                newName: "IX_vouchers_AccountID");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "vouchers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentID",
                table: "vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GuardianID",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Accounts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuardianID",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentID",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_vouchers_StudentID",
                table: "vouchers",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_GuardianID",
                table: "Accounts",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_StudentID",
                table: "Accounts",
                column: "StudentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Guardians_GuardianID",
                table: "Accounts",
                column: "GuardianID",
                principalTable: "Guardians",
                principalColumn: "GuardianID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Students_StudentID",
                table: "Accounts",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "StudentID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students",
                column: "GuardianID",
                principalTable: "Guardians",
                principalColumn: "GuardianID",
                onDelete: ReferentialAction.Restrict);

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
    }
}
