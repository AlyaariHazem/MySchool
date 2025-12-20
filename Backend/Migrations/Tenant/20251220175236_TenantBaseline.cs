using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class TenantBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // English: Baseline migration (no-op). Database already exists.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // English: No-op.
        }
    }
}
