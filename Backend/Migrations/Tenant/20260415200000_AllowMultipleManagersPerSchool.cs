using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant;

/// <summary>
/// <c>IX_Managers_SchoolID</c> was UNIQUE (one manager per school). Multiple managers per school are allowed.
/// </summary>
[Migration("20260415200000_AllowMultipleManagersPerSchool")]
public class AllowMultipleManagersPerSchool : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    WHERE i.name = N'IX_Managers_SchoolID' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND t.name = N'Managers')
BEGIN
    DROP INDEX [IX_Managers_SchoolID] ON [dbo].[Managers];
    CREATE NONCLUSTERED INDEX [IX_Managers_SchoolID] ON [dbo].[Managers]([SchoolID]);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    WHERE i.name = N'IX_Managers_SchoolID' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND t.name = N'Managers')
    DROP INDEX [IX_Managers_SchoolID] ON [dbo].[Managers];

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    WHERE i.name = N'IX_Managers_SchoolID' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND t.name = N'Managers')
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Managers_SchoolID] ON [dbo].[Managers]([SchoolID]);
");
    }
}
