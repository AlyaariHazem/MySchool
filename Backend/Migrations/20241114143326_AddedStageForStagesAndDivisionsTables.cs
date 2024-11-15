using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedStageForStagesAndDivisionsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Divisions",
                newName: "State");

            migrationBuilder.AddColumn<bool>(
                name: "State",
                table: "Classes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Classes");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Divisions",
                newName: "Active");
        }
    }
}
