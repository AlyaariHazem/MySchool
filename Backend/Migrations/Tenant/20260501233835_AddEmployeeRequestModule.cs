using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddEmployeeRequestModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent for tenant DBs where tables may already exist (failed prior run or manual create).
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[EmployeeRequestTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EmployeeRequestTypes] (
        [RequestTypeID] int NOT NULL IDENTITY(1,1),
        [SchoolID] int NOT NULL,
        [Code] nvarchar(64) NOT NULL,
        [Category] int NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [NameAr] nvarchar(128) NULL,
        [Description] nvarchar(2000) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_EmployeeRequestTypes] PRIMARY KEY CLUSTERED ([RequestTypeID]),
        CONSTRAINT [FK_EmployeeRequestTypes_Schools_SchoolID] FOREIGN KEY ([SchoolID]) REFERENCES [dbo].[Schools] ([SchoolID]) ON DELETE NO ACTION
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StaffEmployeeRequests]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StaffEmployeeRequests] (
        [EmployeeRequestID] int NOT NULL IDENTITY(1,1),
        [SchoolID] int NOT NULL,
        [AcademicYearID] int NOT NULL,
        [EmployeeProfileID] int NOT NULL,
        [RequestTypeID] int NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Details] nvarchar(4000) NULL,
        [RequestedAmount] decimal(18,2) NULL,
        [Status] int NOT NULL,
        [SubmittedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        [ResolvedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_StaffEmployeeRequests] PRIMARY KEY CLUSTERED ([EmployeeRequestID]),
        CONSTRAINT [FK_StaffEmployeeRequests_EmployeeProfiles_EmployeeProfileID] FOREIGN KEY ([EmployeeProfileID]) REFERENCES [dbo].[EmployeeProfiles] ([EmployeeProfileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StaffEmployeeRequests_EmployeeRequestTypes_RequestTypeID] FOREIGN KEY ([RequestTypeID]) REFERENCES [dbo].[EmployeeRequestTypes] ([RequestTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StaffEmployeeRequests_Schools_SchoolID] FOREIGN KEY ([SchoolID]) REFERENCES [dbo].[Schools] ([SchoolID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StaffEmployeeRequests_Years_AcademicYearID] FOREIGN KEY ([AcademicYearID]) REFERENCES [dbo].[Years] ([YearID]) ON DELETE NO ACTION
    );
END
");

            migrationBuilder.Sql(@"
                INSERT INTO [EmployeeRequestTypes] ([SchoolID], [Code], [Category], [Name], [NameAr], [Description], [IsActive])
                SELECT s.[SchoolID], 'TOOLS', 0, 'Tools request', N'طلب أدوات', N'Request for devices, tools, or work materials.', 1
                FROM [Schools] s
                WHERE NOT EXISTS (
                    SELECT 1 FROM [EmployeeRequestTypes] rt WHERE rt.[SchoolID] = s.[SchoolID] AND rt.[Code] = 'TOOLS'
                );

                INSERT INTO [EmployeeRequestTypes] ([SchoolID], [Code], [Category], [Name], [NameAr], [Description], [IsActive])
                SELECT s.[SchoolID], 'ADVANCE', 1, 'Salary advance', N'سلفة', N'Advance salary/financial request.', 1
                FROM [Schools] s
                WHERE NOT EXISTS (
                    SELECT 1 FROM [EmployeeRequestTypes] rt WHERE rt.[SchoolID] = s.[SchoolID] AND rt.[Code] = 'ADVANCE'
                );

                INSERT INTO [EmployeeRequestTypes] ([SchoolID], [Code], [Category], [Name], [NameAr], [Description], [IsActive])
                SELECT s.[SchoolID], 'SUPPORT', 2, 'Support request', N'دعم', N'Support request for operational or technical help.', 1
                FROM [Schools] s
                WHERE NOT EXISTS (
                    SELECT 1 FROM [EmployeeRequestTypes] rt WHERE rt.[SchoolID] = s.[SchoolID] AND rt.[Code] = 'SUPPORT'
                );
            ");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StaffRequestApprovalSteps]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StaffRequestApprovalSteps] (
        [RequestApprovalStepID] int NOT NULL IDENTITY(1,1),
        [EmployeeRequestID] int NOT NULL,
        [ApproverEmployeeProfileID] int NOT NULL,
        [StepOrder] int NOT NULL,
        [Decision] int NOT NULL,
        [Comment] nvarchar(2000) NULL,
        [DecidedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_StaffRequestApprovalSteps] PRIMARY KEY CLUSTERED ([RequestApprovalStepID]),
        CONSTRAINT [FK_StaffRequestApprovalSteps_EmployeeProfiles_ApproverEmployeeProfileID] FOREIGN KEY ([ApproverEmployeeProfileID]) REFERENCES [dbo].[EmployeeProfiles] ([EmployeeProfileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StaffRequestApprovalSteps_StaffEmployeeRequests_EmployeeRequestID] FOREIGN KEY ([EmployeeRequestID]) REFERENCES [dbo].[StaffEmployeeRequests] ([EmployeeRequestID]) ON DELETE CASCADE
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StaffRequestDailySummaries]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StaffRequestDailySummaries] (
        [RequestDailySummaryID] int NOT NULL IDENTITY(1,1),
        [EmployeeRequestID] int NOT NULL,
        [SummaryDate] datetime2 NOT NULL,
        [Summary] nvarchar(4000) NOT NULL,
        [ProgressPercent] int NULL,
        [IsFinalForDay] bit NOT NULL,
        [CreatedByEmployeeProfileID] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_StaffRequestDailySummaries] PRIMARY KEY CLUSTERED ([RequestDailySummaryID]),
        CONSTRAINT [FK_StaffRequestDailySummaries_EmployeeProfiles_CreatedByEmployeeProfileID] FOREIGN KEY ([CreatedByEmployeeProfileID]) REFERENCES [dbo].[EmployeeProfiles] ([EmployeeProfileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StaffRequestDailySummaries_StaffEmployeeRequests_EmployeeRequestID] FOREIGN KEY ([EmployeeRequestID]) REFERENCES [dbo].[StaffEmployeeRequests] ([EmployeeRequestID]) ON DELETE CASCADE
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StaffRequestExecutions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StaffRequestExecutions] (
        [RequestExecutionID] int NOT NULL IDENTITY(1,1),
        [EmployeeRequestID] int NOT NULL,
        [Status] int NOT NULL,
        [Notes] nvarchar(2000) NULL,
        [ProgressPercent] int NOT NULL,
        [DueAtUtc] datetime2 NULL,
        [ExecutedAtUtc] datetime2 NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        [ResponsibleEmployeeProfileID] int NULL,
        CONSTRAINT [PK_StaffRequestExecutions] PRIMARY KEY CLUSTERED ([RequestExecutionID]),
        CONSTRAINT [FK_StaffRequestExecutions_EmployeeProfiles_ResponsibleEmployeeProfileID] FOREIGN KEY ([ResponsibleEmployeeProfileID]) REFERENCES [dbo].[EmployeeProfiles] ([EmployeeProfileID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StaffRequestExecutions_StaffEmployeeRequests_EmployeeRequestID] FOREIGN KEY ([EmployeeRequestID]) REFERENCES [dbo].[StaffEmployeeRequests] ([EmployeeRequestID]) ON DELETE CASCADE
    );
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.StaffEmployeeRequests', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffEmployeeRequests_AcademicYearID' AND object_id = OBJECT_ID(N'dbo.StaffEmployeeRequests'))
    CREATE NONCLUSTERED INDEX [IX_StaffEmployeeRequests_AcademicYearID] ON [dbo].[StaffEmployeeRequests]([AcademicYearID]);
IF OBJECT_ID(N'dbo.StaffEmployeeRequests', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffEmployeeRequests_EmployeeProfileID' AND object_id = OBJECT_ID(N'dbo.StaffEmployeeRequests'))
    CREATE NONCLUSTERED INDEX [IX_StaffEmployeeRequests_EmployeeProfileID] ON [dbo].[StaffEmployeeRequests]([EmployeeProfileID]);
IF OBJECT_ID(N'dbo.StaffEmployeeRequests', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffEmployeeRequests_RequestTypeID' AND object_id = OBJECT_ID(N'dbo.StaffEmployeeRequests'))
    CREATE NONCLUSTERED INDEX [IX_StaffEmployeeRequests_RequestTypeID] ON [dbo].[StaffEmployeeRequests]([RequestTypeID]);
IF OBJECT_ID(N'dbo.StaffEmployeeRequests', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffEmployeeRequests_SchoolID_AcademicYearID_EmployeeProfileID_Status' AND object_id = OBJECT_ID(N'dbo.StaffEmployeeRequests'))
    CREATE NONCLUSTERED INDEX [IX_StaffEmployeeRequests_SchoolID_AcademicYearID_EmployeeProfileID_Status] ON [dbo].[StaffEmployeeRequests]([SchoolID], [AcademicYearID], [EmployeeProfileID], [Status]);
IF OBJECT_ID(N'dbo.StaffRequestApprovalSteps', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffRequestApprovalSteps_ApproverEmployeeProfileID' AND object_id = OBJECT_ID(N'dbo.StaffRequestApprovalSteps'))
    CREATE NONCLUSTERED INDEX [IX_StaffRequestApprovalSteps_ApproverEmployeeProfileID] ON [dbo].[StaffRequestApprovalSteps]([ApproverEmployeeProfileID]);
IF OBJECT_ID(N'dbo.StaffRequestApprovalSteps', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffRequestApprovalSteps_EmployeeRequestID_StepOrder' AND object_id = OBJECT_ID(N'dbo.StaffRequestApprovalSteps'))
    CREATE NONCLUSTERED INDEX [IX_StaffRequestApprovalSteps_EmployeeRequestID_StepOrder] ON [dbo].[StaffRequestApprovalSteps]([EmployeeRequestID], [StepOrder]);
IF OBJECT_ID(N'dbo.StaffRequestDailySummaries', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffRequestDailySummaries_CreatedByEmployeeProfileID' AND object_id = OBJECT_ID(N'dbo.StaffRequestDailySummaries'))
    CREATE NONCLUSTERED INDEX [IX_StaffRequestDailySummaries_CreatedByEmployeeProfileID] ON [dbo].[StaffRequestDailySummaries]([CreatedByEmployeeProfileID]);
IF OBJECT_ID(N'dbo.StaffRequestDailySummaries', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffRequestDailySummaries_EmployeeRequestID_SummaryDate_CreatedAtUtc' AND object_id = OBJECT_ID(N'dbo.StaffRequestDailySummaries'))
    CREATE NONCLUSTERED INDEX [IX_StaffRequestDailySummaries_EmployeeRequestID_SummaryDate_CreatedAtUtc] ON [dbo].[StaffRequestDailySummaries]([EmployeeRequestID], [SummaryDate], [CreatedAtUtc]);
IF OBJECT_ID(N'dbo.StaffRequestExecutions', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffRequestExecutions_EmployeeRequestID_UpdatedAtUtc' AND object_id = OBJECT_ID(N'dbo.StaffRequestExecutions'))
    CREATE NONCLUSTERED INDEX [IX_StaffRequestExecutions_EmployeeRequestID_UpdatedAtUtc] ON [dbo].[StaffRequestExecutions]([EmployeeRequestID], [UpdatedAtUtc]);
IF OBJECT_ID(N'dbo.StaffRequestExecutions', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StaffRequestExecutions_ResponsibleEmployeeProfileID' AND object_id = OBJECT_ID(N'dbo.StaffRequestExecutions'))
    CREATE NONCLUSTERED INDEX [IX_StaffRequestExecutions_ResponsibleEmployeeProfileID] ON [dbo].[StaffRequestExecutions]([ResponsibleEmployeeProfileID]);
IF OBJECT_ID(N'dbo.EmployeeRequestTypes', N'U') IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmployeeRequestTypes_SchoolID_Code' AND object_id = OBJECT_ID(N'dbo.EmployeeRequestTypes'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_EmployeeRequestTypes_SchoolID_Code] ON [dbo].[EmployeeRequestTypes]([SchoolID], [Code]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[StaffRequestApprovalSteps]', N'U') IS NOT NULL DROP TABLE [dbo].[StaffRequestApprovalSteps];
IF OBJECT_ID(N'[dbo].[StaffRequestDailySummaries]', N'U') IS NOT NULL DROP TABLE [dbo].[StaffRequestDailySummaries];
IF OBJECT_ID(N'[dbo].[StaffRequestExecutions]', N'U') IS NOT NULL DROP TABLE [dbo].[StaffRequestExecutions];
IF OBJECT_ID(N'[dbo].[StaffEmployeeRequests]', N'U') IS NOT NULL DROP TABLE [dbo].[StaffEmployeeRequests];
IF OBJECT_ID(N'[dbo].[EmployeeRequestTypes]', N'U') IS NOT NULL DROP TABLE [dbo].[EmployeeRequestTypes];
");
        }
    }
}
