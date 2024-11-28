using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangePrimaryKeyInStudentClassFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentClassFees",
                table: "StudentClassFees");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentClassFees",
                table: "StudentClassFees",
                columns: new[] { "ClassID", "StudentID" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassFees_ClassID_FeeID",
                table: "StudentClassFees",
                columns: new[] { "ClassID", "FeeID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentClassFees",
                table: "StudentClassFees");

            migrationBuilder.DropIndex(
                name: "IX_StudentClassFees_ClassID_FeeID",
                table: "StudentClassFees");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentClassFees",
                table: "StudentClassFees",
                columns: new[] { "ClassID", "FeeID" });
        }
    }
}
