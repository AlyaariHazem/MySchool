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
            // New tenant DBs: later migrations (e.g. ReportTemplates → FK_Schools) require this table.
            // Idempotent so existing DBs that already have Schools are unchanged.
            TenantSchoolsBootstrapSql.Apply(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op: do not drop Schools if referenced.
        }
    }
}
