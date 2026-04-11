using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Migrations.Tenant;

/// <summary>
/// Idempotent DDL for tenant tables: (1) core tables referenced by incremental migrations,
/// (2) remaining <see cref="TenantDbContext"/> tables that have no <c>CreateTable</c> migration.
/// Owned name columns use EF Core’s <c>{Navigation}_{Property}</c> pattern (e.g. <c>FullName_FirstName</c>).
/// </summary>
public static class TenantSchoolsBootstrapSql
{
    /// <summary>Run before <see cref="Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate"/> on a new tenant database if needed.</summary>
    public static string CreateSchoolsIfMissingSql => Sql;

    /// <summary>
    /// Idempotent DDL for the exams module (<c>ExamSessions</c>, <c>ExamTypes</c>, <c>ScheduledExams</c>, <c>ExamResults</c>).
    /// Executed separately from <see cref="CreateSchoolsIfMissingSql"/> so tenant DBs that already ran the main bootstrap
    /// still get these tables (see <see cref="Backend.Data.TenantSchemaBootstrapInterceptor"/>).
    /// </summary>
    public static string ExamsModuleEnsureSql => ExamsModuleSql;

    /// <summary>
    /// Idempotent DDL for homework/tasks (<c>HomeworkTasks</c>, <c>HomeworkTaskLinks</c>, <c>HomeworkSubmissions</c>, <c>HomeworkSubmissionFiles</c>).
    /// </summary>
    public static string HomeworkModuleEnsureSql => HomeworkModuleSql;

    private const string ExamsModuleSql = @"
IF OBJECT_ID(N'[dbo].[ExamTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ExamSessions] (
        [ExamSessionID] int NOT NULL IDENTITY(1,1),
        [YearID] int NOT NULL,
        [TermID] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ExamSessions] PRIMARY KEY ([ExamSessionID]),
        CONSTRAINT [FK_ExamSessions_Terms_TermID] FOREIGN KEY ([TermID]) REFERENCES [dbo].[Terms]([TermID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ExamSessions_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_ExamSessions_TermID] ON [dbo].[ExamSessions]([TermID]);
    CREATE NONCLUSTERED INDEX [IX_ExamSessions_YearID] ON [dbo].[ExamSessions]([YearID]);

    CREATE TABLE [dbo].[ExamTypes] (
        [ExamTypeID] int NOT NULL IDENTITY(1,1),
        [Name] nvarchar(max) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ExamTypes] PRIMARY KEY ([ExamTypeID])
    );

    CREATE TABLE [dbo].[ScheduledExams] (
        [ScheduledExamID] int NOT NULL IDENTITY(1,1),
        [ExamSessionID] int NULL,
        [ExamTypeID] int NOT NULL,
        [YearID] int NOT NULL,
        [TermID] int NOT NULL,
        [ClassID] int NOT NULL,
        [DivisionID] int NOT NULL,
        [SubjectID] int NOT NULL,
        [TeacherID] int NOT NULL,
        [ExamDate] datetime2 NOT NULL,
        [StartTime] nvarchar(max) NOT NULL,
        [EndTime] nvarchar(max) NOT NULL,
        [Room] nvarchar(max) NULL,
        [TotalMarks] decimal(18,2) NOT NULL,
        [PassingMarks] decimal(18,2) NOT NULL,
        [SchedulePublished] bit NOT NULL,
        [ResultsPublished] bit NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_ScheduledExams] PRIMARY KEY ([ScheduledExamID]),
        CONSTRAINT [FK_ScheduledExams_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledExams_Divisions_DivisionID] FOREIGN KEY ([DivisionID]) REFERENCES [dbo].[Divisions]([DivisionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledExams_ExamSessions_ExamSessionID] FOREIGN KEY ([ExamSessionID]) REFERENCES [dbo].[ExamSessions]([ExamSessionID]) ON DELETE SET NULL,
        CONSTRAINT [FK_ScheduledExams_ExamTypes_ExamTypeID] FOREIGN KEY ([ExamTypeID]) REFERENCES [dbo].[ExamTypes]([ExamTypeID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledExams_Subjects_SubjectID] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects]([SubjectID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledExams_Teachers_TeacherID] FOREIGN KEY ([TeacherID]) REFERENCES [dbo].[Teachers]([TeacherID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledExams_Terms_TermID] FOREIGN KEY ([TermID]) REFERENCES [dbo].[Terms]([TermID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ScheduledExams_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_ClassID] ON [dbo].[ScheduledExams]([ClassID]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_DivisionID] ON [dbo].[ScheduledExams]([DivisionID]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_ExamDate] ON [dbo].[ScheduledExams]([ExamDate]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_ExamSessionID] ON [dbo].[ScheduledExams]([ExamSessionID]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_ExamTypeID] ON [dbo].[ScheduledExams]([ExamTypeID]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_SubjectID] ON [dbo].[ScheduledExams]([SubjectID]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_TeacherID] ON [dbo].[ScheduledExams]([TeacherID]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_TermID] ON [dbo].[ScheduledExams]([TermID]);
    CREATE NONCLUSTERED INDEX [IX_ScheduledExams_YearID_TermID_ClassID_DivisionID] ON [dbo].[ScheduledExams]([YearID], [TermID], [ClassID], [DivisionID]);

    CREATE TABLE [dbo].[ExamResults] (
        [ExamResultID] int NOT NULL IDENTITY(1,1),
        [ScheduledExamID] int NOT NULL,
        [StudentID] int NOT NULL,
        [Score] decimal(18,2) NULL,
        [IsAbsent] bit NOT NULL,
        [Remarks] nvarchar(max) NULL,
        CONSTRAINT [PK_ExamResults] PRIMARY KEY ([ExamResultID]),
        CONSTRAINT [FK_ExamResults_ScheduledExams_ScheduledExamID] FOREIGN KEY ([ScheduledExamID]) REFERENCES [dbo].[ScheduledExams]([ScheduledExamID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ExamResults_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[Students]([StudentID]) ON DELETE NO ACTION
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ExamResults_ScheduledExamID_StudentID] ON [dbo].[ExamResults]([ScheduledExamID], [StudentID]);
    CREATE NONCLUSTERED INDEX [IX_ExamResults_StudentID] ON [dbo].[ExamResults]([StudentID]);

    SET IDENTITY_INSERT [dbo].[ExamTypes] ON;
    INSERT INTO [dbo].[ExamTypes] ([ExamTypeID], [IsActive], [Name], [SortOrder]) VALUES
    (1, 1, N'Midterm', 1), (2, 1, N'Final', 2), (3, 1, N'Quiz', 3), (4, 1, N'Oral', 4), (5, 1, N'Practical', 5), (6, 1, N'Makeup', 6);
    SET IDENTITY_INSERT [dbo].[ExamTypes] OFF;

    IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260410234115_AddExamsModule')
        INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260410234115_AddExamsModule', N'8.0.10');
END
";

    private const string HomeworkModuleSql = @"
IF OBJECT_ID(N'[dbo].[HomeworkTasks]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HomeworkTasks] (
        [HomeworkTaskID] int NOT NULL IDENTITY(1,1),
        [TeacherID] int NOT NULL,
        [YearID] int NOT NULL,
        [TermID] int NOT NULL,
        [ClassID] int NOT NULL,
        [DivisionID] int NOT NULL,
        [SubjectID] int NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [DueDateUtc] datetime2 NOT NULL,
        [SubmissionRequired] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_HomeworkTasks] PRIMARY KEY ([HomeworkTaskID]),
        CONSTRAINT [FK_HomeworkTasks_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HomeworkTasks_Divisions_DivisionID] FOREIGN KEY ([DivisionID]) REFERENCES [dbo].[Divisions]([DivisionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HomeworkTasks_Subjects_SubjectID] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects]([SubjectID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HomeworkTasks_Teachers_TeacherID] FOREIGN KEY ([TeacherID]) REFERENCES [dbo].[Teachers]([TeacherID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HomeworkTasks_Terms_TermID] FOREIGN KEY ([TermID]) REFERENCES [dbo].[Terms]([TermID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HomeworkTasks_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_ClassID] ON [dbo].[HomeworkTasks]([ClassID]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_DivisionID] ON [dbo].[HomeworkTasks]([DivisionID]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_DueDateUtc] ON [dbo].[HomeworkTasks]([DueDateUtc]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_SubjectID] ON [dbo].[HomeworkTasks]([SubjectID]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_TeacherID] ON [dbo].[HomeworkTasks]([TeacherID]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_TermID] ON [dbo].[HomeworkTasks]([TermID]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_YearID_TermID_ClassID_DivisionID] ON [dbo].[HomeworkTasks]([YearID], [TermID], [ClassID], [DivisionID]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkTasks_YearID] ON [dbo].[HomeworkTasks]([YearID]);

    CREATE TABLE [dbo].[HomeworkTaskLinks] (
        [HomeworkTaskLinkID] int NOT NULL IDENTITY(1,1),
        [HomeworkTaskID] int NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [Label] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_HomeworkTaskLinks] PRIMARY KEY ([HomeworkTaskLinkID]),
        CONSTRAINT [FK_HomeworkTaskLinks_HomeworkTasks_HomeworkTaskID] FOREIGN KEY ([HomeworkTaskID]) REFERENCES [dbo].[HomeworkTasks]([HomeworkTaskID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_HomeworkTaskLinks_HomeworkTaskID] ON [dbo].[HomeworkTaskLinks]([HomeworkTaskID]);

    CREATE TABLE [dbo].[HomeworkSubmissions] (
        [HomeworkSubmissionID] int NOT NULL IDENTITY(1,1),
        [HomeworkTaskID] int NOT NULL,
        [StudentID] int NOT NULL,
        [Status] tinyint NOT NULL,
        [SubmittedAtUtc] datetime2 NULL,
        [AnswerText] nvarchar(max) NULL,
        [TeacherFeedback] nvarchar(max) NULL,
        [Score] decimal(18,2) NULL,
        [FeedbackPublished] bit NOT NULL,
        [ReviewedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_HomeworkSubmissions] PRIMARY KEY ([HomeworkSubmissionID]),
        CONSTRAINT [FK_HomeworkSubmissions_HomeworkTasks_HomeworkTaskID] FOREIGN KEY ([HomeworkTaskID]) REFERENCES [dbo].[HomeworkTasks]([HomeworkTaskID]) ON DELETE CASCADE,
        CONSTRAINT [FK_HomeworkSubmissions_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[Students]([StudentID]) ON DELETE NO ACTION
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_HomeworkSubmissions_HomeworkTaskID_StudentID] ON [dbo].[HomeworkSubmissions]([HomeworkTaskID], [StudentID]);
    CREATE NONCLUSTERED INDEX [IX_HomeworkSubmissions_StudentID] ON [dbo].[HomeworkSubmissions]([StudentID]);

    CREATE TABLE [dbo].[HomeworkSubmissionFiles] (
        [HomeworkSubmissionFileID] int NOT NULL IDENTITY(1,1),
        [HomeworkSubmissionID] int NOT NULL,
        [FileUrl] nvarchar(max) NOT NULL,
        [FileName] nvarchar(max) NULL,
        CONSTRAINT [PK_HomeworkSubmissionFiles] PRIMARY KEY ([HomeworkSubmissionFileID]),
        CONSTRAINT [FK_HomeworkSubmissionFiles_HomeworkSubmissions_HomeworkSubmissionID] FOREIGN KEY ([HomeworkSubmissionID]) REFERENCES [dbo].[HomeworkSubmissions]([HomeworkSubmissionID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_HomeworkSubmissionFiles_HomeworkSubmissionID] ON [dbo].[HomeworkSubmissionFiles]([HomeworkSubmissionID]);

    IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260411130000_AddHomeworkModule')
        INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260411130000_AddHomeworkModule', N'8.0.10');
END
";

    private const string Sql = @"
IF OBJECT_ID(N'[dbo].[Schools]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Schools] (
        [SchoolID] int NOT NULL IDENTITY(1,1),
        [SchoolName] nvarchar(max) NOT NULL,
        [SchoolNameEn] nvarchar(max) NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [SchoolVison] nvarchar(max) NULL,
        [SchoolMission] nvarchar(max) NULL,
        [SchoolGoal] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [Country] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [SchoolPhone] int NOT NULL,
        [Street] nvarchar(max) NOT NULL,
        [SchoolType] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [SchoolCategory] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [Mobile] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [Website] nvarchar(max) NULL,
        [ImageURL] nvarchar(max) NULL,
        [fax] int NULL,
        [zone] nvarchar(max) NULL,
        CONSTRAINT [PK_Schools] PRIMARY KEY ([SchoolID])
    );
END

IF OBJECT_ID(N'[dbo].[Years]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Years] (
        [YearID] int NOT NULL IDENTITY(1,1),
        [Active] bit NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [SchoolID] int NOT NULL,
        [YearDateEnd] datetime2 NULL,
        [YearDateStart] datetime2 NOT NULL,
        CONSTRAINT [PK_Years] PRIMARY KEY ([YearID]),
        CONSTRAINT [FK_Years_Schools_SchoolID] FOREIGN KEY ([SchoolID]) REFERENCES [dbo].[Schools]([SchoolID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_Years_SchoolID] ON [dbo].[Years]([SchoolID]);
END

IF OBJECT_ID(N'[dbo].[Stages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Stages] (
        [StageID] int NOT NULL IDENTITY(1,1),
        [Active] bit NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [Note] nvarchar(max) NULL,
        [StageName] nvarchar(max) NOT NULL,
        [YearID] int NOT NULL,
        CONSTRAINT [PK_Stages] PRIMARY KEY ([StageID]),
        CONSTRAINT [FK_Stages_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_Stages_YearID] ON [dbo].[Stages]([YearID]);
END

IF OBJECT_ID(N'[dbo].[Terms]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Terms] (
        [TermID] int NOT NULL IDENTITY(1,1),
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Terms] PRIMARY KEY ([TermID])
    );
END

IF OBJECT_ID(N'[dbo].[Subjects]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Subjects] (
        [SubjectID] int NOT NULL IDENTITY(1,1),
        [HireDate] datetime2 NOT NULL,
        [Note] nvarchar(max) NULL,
        [SubjectName] nvarchar(max) NOT NULL,
        [SubjectReplacement] nvarchar(max) NULL,
        CONSTRAINT [PK_Subjects] PRIMARY KEY ([SubjectID])
    );
END

IF OBJECT_ID(N'[dbo].[Managers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Managers] (
        [ManagerID] int NOT NULL IDENTITY(1,1),
        [DOB] datetime2 NULL,
        [ImageURL] nvarchar(max) NULL,
        [SchoolID] int NOT NULL,
        [TenantID] int NULL,
        [UserID] nvarchar(max) NULL,
        [FullName_FirstName] nvarchar(max) NOT NULL,
        [FullName_LastName] nvarchar(max) NOT NULL,
        [FullName_MiddleName] nvarchar(max) NULL,
        CONSTRAINT [PK_Managers] PRIMARY KEY ([ManagerID]),
        CONSTRAINT [FK_Managers_Schools_SchoolID] FOREIGN KEY ([SchoolID]) REFERENCES [dbo].[Schools]([SchoolID]) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Managers_SchoolID] ON [dbo].[Managers]([SchoolID]);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Managers_TenantID] ON [dbo].[Managers]([TenantID]) WHERE [TenantID] IS NOT NULL;
END

IF OBJECT_ID(N'[dbo].[Teachers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Teachers] (
        [TeacherID] int NOT NULL IDENTITY(1,1),
        [DOB] datetime2 NULL,
        [ImageURL] nvarchar(max) NULL,
        [ManagerID] int NOT NULL,
        [UserID] nvarchar(max) NULL,
        [FullName_FirstName] nvarchar(max) NOT NULL,
        [FullName_LastName] nvarchar(max) NOT NULL,
        [FullName_MiddleName] nvarchar(max) NULL,
        CONSTRAINT [PK_Teachers] PRIMARY KEY ([TeacherID]),
        CONSTRAINT [FK_Teachers_Managers_ManagerID] FOREIGN KEY ([ManagerID]) REFERENCES [dbo].[Managers]([ManagerID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_Teachers_ManagerID] ON [dbo].[Teachers]([ManagerID]);
END

IF OBJECT_ID(N'[dbo].[Classes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Classes] (
        [ClassID] int NOT NULL IDENTITY(1,1),
        [ClassName] nvarchar(max) NOT NULL,
        [ClassYear] nvarchar(max) NOT NULL,
        [StageID] int NOT NULL,
        [State] bit NOT NULL,
        [TeacherID] int NULL,
        [YearID] int NULL,
        CONSTRAINT [PK_Classes] PRIMARY KEY ([ClassID]),
        CONSTRAINT [FK_Classes_Stages_StageID] FOREIGN KEY ([StageID]) REFERENCES [dbo].[Stages]([StageID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Classes_Teachers_TeacherID] FOREIGN KEY ([TeacherID]) REFERENCES [dbo].[Teachers]([TeacherID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Classes_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_Classes_StageID] ON [dbo].[Classes]([StageID]);
    CREATE NONCLUSTERED INDEX [IX_Classes_TeacherID] ON [dbo].[Classes]([TeacherID]);
    CREATE NONCLUSTERED INDEX [IX_Classes_YearID] ON [dbo].[Classes]([YearID]);
END

IF OBJECT_ID(N'[dbo].[Divisions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Divisions] (
        [DivisionID] int NOT NULL IDENTITY(1,1),
        [ClassID] int NOT NULL,
        [DivisionName] nvarchar(max) NOT NULL,
        [State] bit NOT NULL,
        CONSTRAINT [PK_Divisions] PRIMARY KEY ([DivisionID]),
        CONSTRAINT [FK_Divisions_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_Divisions_ClassID] ON [dbo].[Divisions]([ClassID]);
END

IF OBJECT_ID(N'[dbo].[Guardians]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Guardians] (
        [GuardianID] int NOT NULL IDENTITY(1,1),
        [FullName] nvarchar(max) NOT NULL,
        [GuardianDOB] datetime2 NULL,
        [Type] nvarchar(max) NULL,
        [UserID] nvarchar(max) NULL,
        CONSTRAINT [PK_Guardians] PRIMARY KEY ([GuardianID])
    );
END

IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Students] (
        [StudentID] int NOT NULL IDENTITY(1,1),
        [DivisionID] int NOT NULL,
        [GuardianID] int NOT NULL,
        [ImageURL] nvarchar(max) NULL,
        [PlaceBirth] nvarchar(max) NULL,
        [StudentDOB] datetime2 NULL,
        [UserID] nvarchar(max) NULL,
        [FullName_FirstName] nvarchar(max) NOT NULL,
        [FullName_LastName] nvarchar(max) NOT NULL,
        [FullName_MiddleName] nvarchar(max) NULL,
        [FullNameAlis_FirstNameEng] nvarchar(max) NULL,
        [FullNameAlis_LastNameEng] nvarchar(max) NULL,
        [FullNameAlis_MiddleNameEng] nvarchar(max) NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([StudentID]),
        CONSTRAINT [FK_Students_Divisions_DivisionID] FOREIGN KEY ([DivisionID]) REFERENCES [dbo].[Divisions]([DivisionID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Students_Guardians_GuardianID] FOREIGN KEY ([GuardianID]) REFERENCES [dbo].[Guardians]([GuardianID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_Students_DivisionID] ON [dbo].[Students]([DivisionID]);
    CREATE NONCLUSTERED INDEX [IX_Students_GuardianID] ON [dbo].[Students]([GuardianID]);
END

-- ---------------------------------------------------------------------------
-- Business tables modeled in TenantDbContext but never added by a migration
-- (fees, accounts, grades, attachments, etc.)
-- ---------------------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[TypeAccounts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TypeAccounts] (
        [TypeAccountID] int NOT NULL IDENTITY(1,1),
        [TypeAccountName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TypeAccounts] PRIMARY KEY ([TypeAccountID])
    );
END

IF OBJECT_ID(N'[dbo].[GradeTypes]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[GradeTypes] (
        [GradeTypeID] int NOT NULL IDENTITY(1,1),
        [IsActive] bit NOT NULL,
        [MaxGrade] decimal(18,2) NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_GradeTypes] PRIMARY KEY ([GradeTypeID])
    );
END

IF OBJECT_ID(N'[dbo].[Months]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Months] (
        [MonthID] int NOT NULL IDENTITY(1,1),
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Months] PRIMARY KEY ([MonthID])
    );
END

-- Grade / YearTermMonth FKs require Month rows; seed 12 calendar months when table is empty.
IF OBJECT_ID(N'[dbo].[Months]', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[Months])
BEGIN
    INSERT INTO [dbo].[Months] ([Name]) VALUES
    (N'January'), (N'February'), (N'March'), (N'April'), (N'May'), (N'June'),
    (N'July'), (N'August'), (N'September'), (N'October'), (N'November'), (N'December');
END

IF OBJECT_ID(N'[dbo].[Tenant]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Tenant] (
        [TenantId] int NOT NULL IDENTITY(1,1),
        [ConnectionString] nvarchar(max) NOT NULL,
        [SchoolName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Tenant] PRIMARY KEY ([TenantId])
    );
END

IF OBJECT_ID(N'[dbo].[Fees]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Fees] (
        [FeeID] int NOT NULL IDENTITY(1,1),
        [FeeName] nvarchar(max) NOT NULL,
        [FeeNameAlis] nvarchar(max) NULL,
        [HireDate] datetime2 NOT NULL,
        [Note] nvarchar(max) NULL,
        [State] bit NOT NULL,
        CONSTRAINT [PK_Fees] PRIMARY KEY ([FeeID])
    );
END

IF OBJECT_ID(N'[dbo].[Accounts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Accounts] (
        [AccountID] int NOT NULL IDENTITY(1,1),
        [AccountName] nvarchar(max) NULL,
        [HireDate] datetime2 NOT NULL,
        [Note] nvarchar(max) NULL,
        [OpenBalance] decimal(18,2) NULL,
        [State] bit NOT NULL,
        [TypeAccountID] int NOT NULL,
        [TypeOpenBalance] bit NOT NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([AccountID]),
        CONSTRAINT [FK_Accounts_TypeAccounts_TypeAccountID] FOREIGN KEY ([TypeAccountID]) REFERENCES [dbo].[TypeAccounts]([TypeAccountID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_Accounts_TypeAccountID] ON [dbo].[Accounts]([TypeAccountID]);
END

IF OBJECT_ID(N'[dbo].[FeeClass]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FeeClass] (
        [FeeClassID] int NOT NULL IDENTITY(1,1),
        [Amount] float NULL,
        [ClassID] int NOT NULL,
        [FeeID] int NOT NULL,
        [Mandatory] bit NOT NULL,
        CONSTRAINT [PK_FeeClass] PRIMARY KEY ([FeeClassID]),
        CONSTRAINT [FK_FeeClass_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE CASCADE,
        CONSTRAINT [FK_FeeClass_Fees_FeeID] FOREIGN KEY ([FeeID]) REFERENCES [dbo].[Fees]([FeeID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_FeeClass_ClassID] ON [dbo].[FeeClass]([ClassID]);
    CREATE NONCLUSTERED INDEX [IX_FeeClass_FeeID] ON [dbo].[FeeClass]([FeeID]);
END

IF OBJECT_ID(N'[dbo].[CoursePlans]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CoursePlans] (
        [YearID] int NOT NULL,
        [TeacherID] int NOT NULL,
        [ClassID] int NOT NULL,
        [DivisionID] int NOT NULL,
        [SubjectID] int NOT NULL,
        [TermID] int NOT NULL,
        CONSTRAINT [PK_CoursePlans] PRIMARY KEY ([YearID], [TeacherID], [ClassID], [DivisionID], [SubjectID], [TermID]),
        CONSTRAINT [FK_CoursePlans_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoursePlans_Teachers_TeacherID] FOREIGN KEY ([TeacherID]) REFERENCES [dbo].[Teachers]([TeacherID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CoursePlans_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CoursePlans_Divisions_DivisionID] FOREIGN KEY ([DivisionID]) REFERENCES [dbo].[Divisions]([DivisionID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CoursePlans_Subjects_SubjectID] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects]([SubjectID]) ON DELETE CASCADE,
        CONSTRAINT [FK_CoursePlans_Terms_TermID] FOREIGN KEY ([TermID]) REFERENCES [dbo].[Terms]([TermID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_CoursePlans_ClassID] ON [dbo].[CoursePlans]([ClassID]);
    CREATE NONCLUSTERED INDEX [IX_CoursePlans_DivisionID] ON [dbo].[CoursePlans]([DivisionID]);
    CREATE NONCLUSTERED INDEX [IX_CoursePlans_SubjectID] ON [dbo].[CoursePlans]([SubjectID]);
    CREATE NONCLUSTERED INDEX [IX_CoursePlans_TeacherID] ON [dbo].[CoursePlans]([TeacherID]);
    CREATE NONCLUSTERED INDEX [IX_CoursePlans_TermID] ON [dbo].[CoursePlans]([TermID]);
END

IF OBJECT_ID(N'[dbo].[Curriculums]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Curriculums] (
        [SubjectID] int NOT NULL,
        [ClassID] int NOT NULL,
        [CurriculumName] nvarchar(max) NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_Curriculums] PRIMARY KEY ([SubjectID], [ClassID]),
        CONSTRAINT [FK_Curriculums_Subjects_SubjectID] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects]([SubjectID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Curriculums_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_Curriculums_ClassID] ON [dbo].[Curriculums]([ClassID]);
END

IF OBJECT_ID(N'[dbo].[MonthlyGrades]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MonthlyGrades] (
        [StudentID] int NOT NULL,
        [YearID] int NOT NULL,
        [SubjectID] int NOT NULL,
        [MonthID] int NOT NULL,
        [GradeTypeID] int NOT NULL,
        [ClassID] int NOT NULL,
        [TermID] int NOT NULL,
        [Grade] decimal(18,2) NULL,
        CONSTRAINT [PK_MonthlyGrades] PRIMARY KEY ([StudentID], [YearID], [SubjectID], [MonthID], [GradeTypeID], [ClassID], [TermID]),
        CONSTRAINT [FK_MonthlyGrades_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MonthlyGrades_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[Students]([StudentID]) ON DELETE CASCADE,
        CONSTRAINT [FK_MonthlyGrades_Subjects_SubjectID] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects]([SubjectID]) ON DELETE CASCADE,
        CONSTRAINT [FK_MonthlyGrades_Months_MonthID] FOREIGN KEY ([MonthID]) REFERENCES [dbo].[Months]([MonthID]) ON DELETE CASCADE,
        CONSTRAINT [FK_MonthlyGrades_GradeTypes_GradeTypeID] FOREIGN KEY ([GradeTypeID]) REFERENCES [dbo].[GradeTypes]([GradeTypeID]) ON DELETE CASCADE,
        CONSTRAINT [FK_MonthlyGrades_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MonthlyGrades_Terms_TermID] FOREIGN KEY ([TermID]) REFERENCES [dbo].[Terms]([TermID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_MonthlyGrades_ClassID] ON [dbo].[MonthlyGrades]([ClassID]);
    CREATE NONCLUSTERED INDEX [IX_MonthlyGrades_GradeTypeID] ON [dbo].[MonthlyGrades]([GradeTypeID]);
    CREATE NONCLUSTERED INDEX [IX_MonthlyGrades_MonthID] ON [dbo].[MonthlyGrades]([MonthID]);
    CREATE NONCLUSTERED INDEX [IX_MonthlyGrades_SubjectID] ON [dbo].[MonthlyGrades]([SubjectID]);
    CREATE NONCLUSTERED INDEX [IX_MonthlyGrades_TermID] ON [dbo].[MonthlyGrades]([TermID]);
    CREATE NONCLUSTERED INDEX [IX_MonthlyGrades_YearID] ON [dbo].[MonthlyGrades]([YearID]);
END

IF OBJECT_ID(N'[dbo].[Salarys]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Salarys] (
        [SalaryID] int NOT NULL IDENTITY(1,1),
        [Note] nvarchar(max) NULL,
        [SalaryAmount] int NOT NULL,
        [SalaryHireDate] datetime2 NOT NULL,
        [SalaryMonth] datetime2 NOT NULL,
        [TeacherID] int NOT NULL,
        CONSTRAINT [PK_Salarys] PRIMARY KEY ([SalaryID]),
        CONSTRAINT [FK_Salarys_Teachers_TeacherID] FOREIGN KEY ([TeacherID]) REFERENCES [dbo].[Teachers]([TeacherID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_Salarys_TeacherID] ON [dbo].[Salarys]([TeacherID]);
END

IF OBJECT_ID(N'[dbo].[AccountStudentGuardians]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AccountStudentGuardians] (
        [AccountStudentGuardianID] int NOT NULL IDENTITY(1,1),
        [AccountID] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [GuardianID] int NOT NULL,
        [StudentID] int NOT NULL,
        CONSTRAINT [PK_AccountStudentGuardians] PRIMARY KEY ([AccountStudentGuardianID]),
        CONSTRAINT [FK_AccountStudentGuardians_Accounts_AccountID] FOREIGN KEY ([AccountID]) REFERENCES [dbo].[Accounts]([AccountID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccountStudentGuardians_Guardians_GuardianID] FOREIGN KEY ([GuardianID]) REFERENCES [dbo].[Guardians]([GuardianID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccountStudentGuardians_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[Students]([StudentID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_AccountStudentGuardians_AccountID] ON [dbo].[AccountStudentGuardians]([AccountID]);
    CREATE NONCLUSTERED INDEX [IX_AccountStudentGuardians_GuardianID] ON [dbo].[AccountStudentGuardians]([GuardianID]);
    CREATE NONCLUSTERED INDEX [IX_AccountStudentGuardians_StudentID] ON [dbo].[AccountStudentGuardians]([StudentID]);
END

IF OBJECT_ID(N'[dbo].[StudentClassFees]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[StudentClassFees] (
        [StudentClassFeesID] int NOT NULL IDENTITY(1,1),
        [AmountDiscount] decimal(18,2) NULL,
        [FeeClassID] int NOT NULL,
        [Mandatory] bit NULL,
        [NoteDiscount] nvarchar(max) NULL,
        [StudentID] int NOT NULL,
        CONSTRAINT [PK_StudentClassFees] PRIMARY KEY ([StudentClassFeesID]),
        CONSTRAINT [FK_StudentClassFees_FeeClass_FeeClassID] FOREIGN KEY ([FeeClassID]) REFERENCES [dbo].[FeeClass]([FeeClassID]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentClassFees_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[Students]([StudentID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_StudentClassFees_FeeClassID] ON [dbo].[StudentClassFees]([FeeClassID]);
    CREATE NONCLUSTERED INDEX [IX_StudentClassFees_StudentID] ON [dbo].[StudentClassFees]([StudentID]);
END

IF OBJECT_ID(N'[dbo].[TermlyGrades]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TermlyGrades] (
        [TermlyGradeID] int NOT NULL IDENTITY(1,1),
        [ClassID] int NOT NULL,
        [Grade] decimal(18,2) NULL,
        [Note] nvarchar(max) NULL,
        [StudentID] int NOT NULL,
        [SubjectID] int NOT NULL,
        [TermID] int NOT NULL,
        [YearID] int NOT NULL,
        CONSTRAINT [PK_TermlyGrades] PRIMARY KEY ([TermlyGradeID]),
        CONSTRAINT [FK_TermlyGrades_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TermlyGrades_Classes_ClassID] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[Classes]([ClassID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TermlyGrades_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[Students]([StudentID]) ON DELETE CASCADE,
        CONSTRAINT [FK_TermlyGrades_Subjects_SubjectID] FOREIGN KEY ([SubjectID]) REFERENCES [dbo].[Subjects]([SubjectID]) ON DELETE CASCADE,
        CONSTRAINT [FK_TermlyGrades_Terms_TermID] FOREIGN KEY ([TermID]) REFERENCES [dbo].[Terms]([TermID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_TermlyGrades_ClassID] ON [dbo].[TermlyGrades]([ClassID]);
    CREATE NONCLUSTERED INDEX [IX_TermlyGrades_StudentID] ON [dbo].[TermlyGrades]([StudentID]);
    CREATE NONCLUSTERED INDEX [IX_TermlyGrades_SubjectID] ON [dbo].[TermlyGrades]([SubjectID]);
    CREATE NONCLUSTERED INDEX [IX_TermlyGrades_TermID] ON [dbo].[TermlyGrades]([TermID]);
    CREATE NONCLUSTERED INDEX [IX_TermlyGrades_YearID] ON [dbo].[TermlyGrades]([YearID]);
END

IF OBJECT_ID(N'[dbo].[Vouchers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Vouchers] (
        [VoucherID] int NOT NULL IDENTITY(1,1),
        [AccountStudentGuardianID] int NOT NULL,
        [HireDate] datetime2 NOT NULL,
        [Note] nvarchar(max) NULL,
        [PayBy] nvarchar(max) NOT NULL,
        [Receipt] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Vouchers] PRIMARY KEY ([VoucherID]),
        CONSTRAINT [FK_Vouchers_AccountStudentGuardians_AccountStudentGuardianID] FOREIGN KEY ([AccountStudentGuardianID]) REFERENCES [dbo].[AccountStudentGuardians]([AccountStudentGuardianID]) ON DELETE NO ACTION
    );
    CREATE NONCLUSTERED INDEX [IX_Vouchers_AccountStudentGuardianID] ON [dbo].[Vouchers]([AccountStudentGuardianID]);
END

IF OBJECT_ID(N'[dbo].[Attachments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Attachments] (
        [AttachmentID] int NOT NULL IDENTITY(1,1),
        [AttachmentURL] nvarchar(max) NOT NULL,
        [StudentID] int NULL,
        [VoucherID] int NULL,
        CONSTRAINT [PK_Attachments] PRIMARY KEY ([AttachmentID]),
        CONSTRAINT [FK_Attachments_Students_StudentID] FOREIGN KEY ([StudentID]) REFERENCES [dbo].[Students]([StudentID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Attachments_Vouchers_VoucherID] FOREIGN KEY ([VoucherID]) REFERENCES [dbo].[Vouchers]([VoucherID]) ON DELETE SET NULL
    );
    CREATE NONCLUSTERED INDEX [IX_Attachments_StudentID] ON [dbo].[Attachments]([StudentID]);
    CREATE NONCLUSTERED INDEX [IX_Attachments_VoucherID] ON [dbo].[Attachments]([VoucherID]);
END

IF OBJECT_ID(N'[dbo].[YearTermMonths]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[YearTermMonths] (
        [YearID] int NOT NULL,
        [TermID] int NOT NULL,
        [MonthID] int NOT NULL,
        CONSTRAINT [PK_YearTermMonths] PRIMARY KEY ([YearID], [TermID], [MonthID]),
        CONSTRAINT [FK_YearTermMonths_Years_YearID] FOREIGN KEY ([YearID]) REFERENCES [dbo].[Years]([YearID]) ON DELETE CASCADE,
        CONSTRAINT [FK_YearTermMonths_Terms_TermID] FOREIGN KEY ([TermID]) REFERENCES [dbo].[Terms]([TermID]) ON DELETE CASCADE,
        CONSTRAINT [FK_YearTermMonths_Months_MonthID] FOREIGN KEY ([MonthID]) REFERENCES [dbo].[Months]([MonthID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_YearTermMonths_MonthID] ON [dbo].[YearTermMonths]([MonthID]);
    CREATE NONCLUSTERED INDEX [IX_YearTermMonths_TermID] ON [dbo].[YearTermMonths]([TermID]);
END

-- EF HasData for TypeAccounts / GradeTypes (empty new DBs only)
IF OBJECT_ID(N'[dbo].[TypeAccounts]', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[TypeAccounts])
BEGIN
    SET IDENTITY_INSERT [dbo].[TypeAccounts] ON;
    INSERT INTO [dbo].[TypeAccounts] ([TypeAccountID], [TypeAccountName]) VALUES
    (1, N'Guardain'), (2, N'School'), (3, N'Branches'), (4, N'Funds'), (5, N'Employees'), (6, N'Banks');
    SET IDENTITY_INSERT [dbo].[TypeAccounts] OFF;
END

IF OBJECT_ID(N'[dbo].[GradeTypes]', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[GradeTypes])
BEGIN
    SET IDENTITY_INSERT [dbo].[GradeTypes] ON;
    INSERT INTO [dbo].[GradeTypes] ([GradeTypeID], [IsActive], [MaxGrade], [Name]) VALUES
    (1, 1, 20.00, N'Assignments'),
    (2, 1, 20.00, N'Attendance'),
    (3, 1, 10.00, N'Participation'),
    (4, 1, 10.00, N'Oral'),
    (5, 1, 40.00, N'Exam'),
    (6, 0, 20.00, N'work'),
    (7, 0, 30.00, N'lab'),
    (8, 0, 20.00, N'skills');
    SET IDENTITY_INSERT [dbo].[GradeTypes] OFF;
END

-- Repair tenant DBs created with an older bootstrap that used unprefixed name columns.
IF OBJECT_ID(N'[dbo].[Managers]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Managers', N'FullName_FirstName') IS NULL
   AND COL_LENGTH(N'dbo.Managers', N'FirstName') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.Managers.FirstName', N'FullName_FirstName', N'COLUMN';
    EXEC sp_rename N'dbo.Managers.LastName', N'FullName_LastName', N'COLUMN';
    EXEC sp_rename N'dbo.Managers.MiddleName', N'FullName_MiddleName', N'COLUMN';
END

IF OBJECT_ID(N'[dbo].[Teachers]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Teachers', N'FullName_FirstName') IS NULL
   AND COL_LENGTH(N'dbo.Teachers', N'FirstName') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.Teachers.FirstName', N'FullName_FirstName', N'COLUMN';
    EXEC sp_rename N'dbo.Teachers.LastName', N'FullName_LastName', N'COLUMN';
    EXEC sp_rename N'dbo.Teachers.MiddleName', N'FullName_MiddleName', N'COLUMN';
END

IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Students', N'FullName_FirstName') IS NULL
   AND COL_LENGTH(N'dbo.Students', N'FirstName') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.Students.FirstName', N'FullName_FirstName', N'COLUMN';
    EXEC sp_rename N'dbo.Students.LastName', N'FullName_LastName', N'COLUMN';
    EXEC sp_rename N'dbo.Students.MiddleName', N'FullName_MiddleName', N'COLUMN';
END

IF OBJECT_ID(N'[dbo].[Students]', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Students', N'FullNameAlis_FirstNameEng') IS NULL
   AND COL_LENGTH(N'dbo.Students', N'FirstNameEng') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.Students.FirstNameEng', N'FullNameAlis_FirstNameEng', N'COLUMN';
    EXEC sp_rename N'dbo.Students.LastNameEng', N'FullNameAlis_LastNameEng', N'COLUMN';
    EXEC sp_rename N'dbo.Students.MiddleNameEng', N'FullNameAlis_MiddleNameEng', N'COLUMN';
END
";

    internal static void Apply(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Sql);
}

