/*
  =============================================================================
  WHY THESE OBJECTS EXIST IN A "TENANT" DATABASE
  =============================================================================
  Centralized-auth design keeps Identity + Tenants + UserTenants ONLY on the
  master database (SqlAdminConnection / School).

  If you still see AspNet* / Tenants / UserTenants / RefreshTokens in a tenant
  database (e.g. School_6b23a052), that database was migrated at some point
  with the FULL ApplicationDbContext (DatabaseContext), e.g. old code that did:
      new DatabaseContext(tenantConnectionString).Database.Migrate()
  or running:
      dotnet ef database update --context DatabaseContext
  while pointing at a tenant connection string.

  TenantDbContext migrations (Backend/Migrations/Tenant) do NOT create those
  tables. Current provisioning uses TenantDbContext.Migrate only.

  =============================================================================
  WHAT THIS SCRIPT DOES
  =============================================================================
  Drops master-only tables from ONE tenant database if present (safe IF EXISTS).
  Run against each tenant DB that was polluted. Backup first.

  Optional: After cleanup, you may remove rows from __EFMigrationsHistory that
  belong ONLY to DatabaseContext (master) migrations so the history matches
  reality—only if you know what you are doing; wrong deletes can confuse EF.
  =============================================================================
*/

/* Master-only: tenancy + membership + subscriptions (same model as master DB) */
IF OBJECT_ID(N'dbo.UserTenants', N'U') IS NOT NULL DROP TABLE dbo.UserTenants;
IF OBJECT_ID(N'dbo.Subscriptions', N'U') IS NOT NULL DROP TABLE dbo.Subscriptions;
IF OBJECT_ID(N'dbo.TenantSettings', N'U') IS NOT NULL DROP TABLE dbo.TenantSettings;

/*
  Tenants is often referenced by business tables when DatabaseContext was applied here
  (e.g. Managers.TenantID -> Tenants). Drop those FKs first or DROP TABLE Tenants fails (3726).
*/
DECLARE @dropFk nvarchar(max) = N'';
SELECT @dropFk = @dropFk + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
    + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys AS fk
WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.Tenants');
IF LEN(@dropFk) > 0 EXEC sp_executesql @dropFk;

IF OBJECT_ID(N'dbo.Tenants', N'U') IS NOT NULL DROP TABLE dbo.Tenants;

/* RefreshTokens (FK to AspNetUsers) — before AspNetUsers */
IF OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NOT NULL DROP TABLE dbo.RefreshTokens;

/*
  Business tables may still have FK -> AspNetUsers (Guardians/Teachers/Students/Managers).
  Drop those constraints before removing Identity tables.
*/
SET @dropFk = N'';
SELECT @dropFk = @dropFk + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id))
    + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
    + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys AS fk
WHERE fk.referenced_object_id = OBJECT_ID(N'dbo.AspNetUsers');
IF LEN(@dropFk) > 0 EXEC sp_executesql @dropFk;

/* ASP.NET Core Identity */
IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserTokens;
IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserRoles;
IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserClaims;
IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserLogins;
IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NOT NULL DROP TABLE dbo.AspNetRoleClaims;
IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL DROP TABLE dbo.AspNetUsers;
IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NOT NULL DROP TABLE dbo.AspNetRoles;
