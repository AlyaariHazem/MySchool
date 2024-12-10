using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationShipBetweenGuardainStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuardianID",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 1012);

            migrationBuilder.CreateIndex(
                name: "IX_Students_GuardianID",
                table: "Students",
                column: "GuardianID");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students",
                column: "GuardianID",
                principalTable: "Guardians",
                principalColumn: "GuardianID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Guardians_GuardianID",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_GuardianID",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "GuardianID",
                table: "Students");
        }
    }
}
