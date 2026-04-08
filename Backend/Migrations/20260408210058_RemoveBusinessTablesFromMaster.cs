using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBusinessTablesFromMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables from tenant-only migrations sometimes applied to the master DB; drop first (FK order).
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[NotificationDeliveries]', N'U') IS NOT NULL DROP TABLE [dbo].[NotificationDeliveries];
                IF OBJECT_ID(N'[dbo].[NotificationMessages]', N'U') IS NOT NULL DROP TABLE [dbo].[NotificationMessages];
                IF OBJECT_ID(N'[dbo].[AuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[AuditLogs];
                IF OBJECT_ID(N'[dbo].[Attendances]', N'U') IS NOT NULL DROP TABLE [dbo].[Attendances];
                IF OBJECT_ID(N'[dbo].[ReportTemplates]', N'U') IS NOT NULL DROP TABLE [dbo].[ReportTemplates];
                IF OBJECT_ID(N'[dbo].[WeeklySchedules]', N'U') IS NOT NULL DROP TABLE [dbo].[WeeklySchedules];
                """);

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "CoursePlans");

            migrationBuilder.DropTable(
                name: "Curriculums");

            migrationBuilder.DropTable(
                name: "MonthlyGrades");

            migrationBuilder.DropTable(
                name: "Salarys");

            migrationBuilder.DropTable(
                name: "StudentClassFees");

            migrationBuilder.DropTable(
                name: "TermlyGrades");

            migrationBuilder.DropTable(
                name: "YearTermMonths");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropTable(
                name: "GradeTypes");

            migrationBuilder.DropTable(
                name: "FeeClass");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Months");

            migrationBuilder.DropTable(
                name: "Terms");

            migrationBuilder.DropTable(
                name: "AccountStudentGuardians");

            migrationBuilder.DropTable(
                name: "Fees");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "TypeAccounts");

            migrationBuilder.DropTable(
                name: "Divisions");

            migrationBuilder.DropTable(
                name: "Guardians");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Stages");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Years");

            migrationBuilder.DropTable(
                name: "Managers");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a1b2c3d4e5f647899876543210fedcba", new DateTime(2026, 4, 8, 21, 0, 57, 735, DateTimeKind.Utc).AddTicks(6187), "AQAAAAIAAYagAAAAEIauMaht3feaQi/+ngEZRiZXg+qA2bn7z77pRg1JMoNUCOspaR3/rJtVTvnkdj0VDw==", "f9a7b3c2d1e04a5b9c8d7e6f5a4b3c2d" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fees",
                columns: table => new
                {
                    FeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FeeNameAlis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fees", x => x.FeeID);
                });

            migrationBuilder.CreateTable(
                name: "GradeTypes",
                columns: table => new
                {
                    GradeTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxGrade = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeTypes", x => x.GradeTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Guardians",
                columns: table => new
                {
                    GuardianID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuardianDOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guardians", x => x.GuardianID);
                    table.ForeignKey(
                        name: "FK_Guardians_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Months",
                columns: table => new
                {
                    MonthID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Months", x => x.MonthID);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    SchoolID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolGoal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolMission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolNameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolPhone = table.Column<int>(type: "int", nullable: false),
                    SchoolType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolVison = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fax = table.Column<int>(type: "int", nullable: true),
                    zone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.SchoolID);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectReplacement = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectID);
                });

            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    TermID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.TermID);
                });

            migrationBuilder.CreateTable(
                name: "TypeAccounts",
                columns: table => new
                {
                    TypeAccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeAccountName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeAccounts", x => x.TypeAccountID);
                });

            migrationBuilder.CreateTable(
                name: "Managers",
                columns: table => new
                {
                    ManagerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    TenantID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Managers", x => x.ManagerID);
                    table.ForeignKey(
                        name: "FK_Managers_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Managers_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Managers_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Years",
                columns: table => new
                {
                    YearID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolID = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YearDateEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YearDateStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Years", x => x.YearID);
                    table.ForeignKey(
                        name: "FK_Years_Schools_SchoolID",
                        column: x => x.SchoolID,
                        principalTable: "Schools",
                        principalColumn: "SchoolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeAccountID = table.Column<int>(type: "int", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpenBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    State = table.Column<bool>(type: "bit", nullable: false),
                    TypeOpenBalance = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_Accounts_TypeAccounts_TypeAccountID",
                        column: x => x.TypeAccountID,
                        principalTable: "TypeAccounts",
                        principalColumn: "TypeAccountID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    TeacherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagerID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.TeacherID);
                    table.ForeignKey(
                        name: "FK_Teachers_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teachers_Managers_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "Managers",
                        principalColumn: "ManagerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stages",
                columns: table => new
                {
                    StageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StageName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.StageID);
                    table.ForeignKey(
                        name: "FK_Stages_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YearTermMonths",
                columns: table => new
                {
                    YearID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    MonthID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearTermMonths", x => new { x.YearID, x.TermID, x.MonthID });
                    table.ForeignKey(
                        name: "FK_YearTermMonths_Months_MonthID",
                        column: x => x.MonthID,
                        principalTable: "Months",
                        principalColumn: "MonthID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YearTermMonths_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YearTermMonths_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Salarys",
                columns: table => new
                {
                    SalaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalaryAmount = table.Column<int>(type: "int", nullable: false),
                    SalaryHireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalaryMonth = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salarys", x => x.SalaryID);
                    table.ForeignKey(
                        name: "FK_Salarys_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageID = table.Column<int>(type: "int", nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: true),
                    YearID = table.Column<int>(type: "int", nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ClassID);
                    table.ForeignKey(
                        name: "FK_Classes_Stages_StageID",
                        column: x => x.StageID,
                        principalTable: "Stages",
                        principalColumn: "StageID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Classes_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Curriculums",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    CurriculumName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curriculums", x => new { x.SubjectID, x.ClassID });
                    table.ForeignKey(
                        name: "FK_Curriculums_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Curriculums_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    DivisionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    DivisionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.DivisionID);
                    table.ForeignKey(
                        name: "FK_Divisions_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeeClass",
                columns: table => new
                {
                    FeeClassID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    FeeID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: true),
                    Mandatory = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeClass", x => x.FeeClassID);
                    table.ForeignKey(
                        name: "FK_FeeClass_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeeClass_Fees_FeeID",
                        column: x => x.FeeID,
                        principalTable: "Fees",
                        principalColumn: "FeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoursePlans",
                columns: table => new
                {
                    YearID = table.Column<int>(type: "int", nullable: false),
                    TeacherID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    DivisionID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePlans", x => new { x.YearID, x.TeacherID, x.ClassID, x.DivisionID, x.SubjectID });
                    table.ForeignKey(
                        name: "FK_CoursePlans_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Teachers_TeacherID",
                        column: x => x.TeacherID,
                        principalTable: "Teachers",
                        principalColumn: "TeacherID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlans_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    DivisionID = table.Column<int>(type: "int", nullable: false),
                    GuardianID = table.Column<int>(type: "int", nullable: false, defaultValue: 1012),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentDOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FullName_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName_MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameAlis_FirstNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameAlis_LastNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameAlis_MiddleNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentID);
                    table.ForeignKey(
                        name: "FK_Students_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Students_Divisions_DivisionID",
                        column: x => x.DivisionID,
                        principalTable: "Divisions",
                        principalColumn: "DivisionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Guardians_GuardianID",
                        column: x => x.GuardianID,
                        principalTable: "Guardians",
                        principalColumn: "GuardianID",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "MonthlyGrades",
                columns: table => new
                {
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    MonthID = table.Column<int>(type: "int", nullable: false),
                    GradeTypeID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyGrades", x => new { x.StudentID, x.YearID, x.SubjectID, x.MonthID, x.GradeTypeID, x.ClassID, x.TermID });
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_GradeTypes_GradeTypeID",
                        column: x => x.GradeTypeID,
                        principalTable: "GradeTypes",
                        principalColumn: "GradeTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Months_MonthID",
                        column: x => x.MonthID,
                        principalTable: "Months",
                        principalColumn: "MonthID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyGrades_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentClassFees",
                columns: table => new
                {
                    StudentClassFeesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeClassID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    AmountDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Mandatory = table.Column<bool>(type: "bit", nullable: true),
                    NoteDiscount = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClassFees", x => x.StudentClassFeesID);
                    table.ForeignKey(
                        name: "FK_StudentClassFees_FeeClass_FeeClassID",
                        column: x => x.FeeClassID,
                        principalTable: "FeeClass",
                        principalColumn: "FeeClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentClassFees_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TermlyGrades",
                columns: table => new
                {
                    TermlyGradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    TermID = table.Column<int>(type: "int", nullable: false),
                    YearID = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermlyGrades", x => x.TermlyGradeID);
                    table.ForeignKey(
                        name: "FK_TermlyGrades_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermlyGrades_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermlyGrades_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermlyGrades_Terms_TermID",
                        column: x => x.TermID,
                        principalTable: "Terms",
                        principalColumn: "TermID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TermlyGrades_Years_YearID",
                        column: x => x.YearID,
                        principalTable: "Years",
                        principalColumn: "YearID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    VoucherID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountStudentGuardianID = table.Column<int>(type: "int", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Receipt = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.VoucherID);
                    table.ForeignKey(
                        name: "FK_Vouchers_AccountStudentGuardians_AccountStudentGuardianID",
                        column: x => x.AccountStudentGuardianID,
                        principalTable: "AccountStudentGuardians",
                        principalColumn: "AccountStudentGuardianID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AttachmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: true),
                    VoucherID = table.Column<int>(type: "int", nullable: true),
                    AttachmentURL = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentID);
                    table.ForeignKey(
                        name: "FK_Attachments_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachments_Vouchers_VoucherID",
                        column: x => x.VoucherID,
                        principalTable: "Vouchers",
                        principalColumn: "VoucherID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "007266f8-a4b4-4b9e-a8d2-3e0a6f9df5ec",
                columns: new[] { "ConcurrencyStamp", "HireDate", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8469deea-476c-417f-9df1-93d69804b391", new DateTime(2026, 4, 8, 23, 37, 53, 376, DateTimeKind.Local).AddTicks(3259), "AQAAAAIAAYagAAAAEAmcG/oiZ7jZDk4rH/s+lr09UZkSx+VYa2Kh9Xr/+M7Wc0oRwaKhGKcpXrQ9S1E1bg==", "a99823cd-8c42-4adc-9e19-2160bed1359f" });

            migrationBuilder.InsertData(
                table: "GradeTypes",
                columns: new[] { "GradeTypeID", "IsActive", "MaxGrade", "Name" },
                values: new object[,]
                {
                    { 1, true, 20m, "Assignments" },
                    { 2, true, 20m, "Attendance" },
                    { 3, true, 10m, "Participation" },
                    { 4, true, 10m, "Oral" },
                    { 5, true, 40m, "Exam" },
                    { 6, false, 20m, "work" },
                    { 7, false, 30m, "lab" },
                    { 8, false, 20m, "skills" }
                });

            migrationBuilder.InsertData(
                table: "Months",
                columns: new[] { "MonthID", "Name" },
                values: new object[,]
                {
                    { 1, "يناير" },
                    { 2, "فبراير" },
                    { 3, "مارس" },
                    { 4, "أبريل" },
                    { 5, "مايو" },
                    { 6, "يونيو" },
                    { 7, "يوليو" },
                    { 8, "أغسطس" },
                    { 9, "سبتمبر" },
                    { 10, "أكتوبر" },
                    { 11, "نوفمبر" },
                    { 12, "ديسمبر" }
                });

            migrationBuilder.InsertData(
                table: "Terms",
                columns: new[] { "TermID", "Name" },
                values: new object[,]
                {
                    { 1, "الأول" },
                    { 2, "الثاني" }
                });

            migrationBuilder.InsertData(
                table: "TypeAccounts",
                columns: new[] { "TypeAccountID", "TypeAccountName" },
                values: new object[,]
                {
                    { 1, "Guardain" },
                    { 2, "School" },
                    { 3, "Branches" },
                    { 4, "Funds" },
                    { 5, "Employees" },
                    { 6, "Banks" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TypeAccountID",
                table: "Accounts",
                column: "TypeAccountID");

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

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_StudentID",
                table: "Attachments",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_VoucherID",
                table: "Attachments",
                column: "VoucherID");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_StageID",
                table: "Classes",
                column: "StageID");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherID",
                table: "Classes",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_YearID",
                table: "Classes",
                column: "YearID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_ClassID",
                table: "CoursePlans",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_DivisionID",
                table: "CoursePlans",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_SubjectID",
                table: "CoursePlans",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_TeacherID",
                table: "CoursePlans",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlans_TermID",
                table: "CoursePlans",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_Curriculums_ClassID",
                table: "Curriculums",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_ClassID",
                table: "Divisions",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_FeeClass_ClassID",
                table: "FeeClass",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_FeeClass_FeeID",
                table: "FeeClass",
                column: "FeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Guardians_UserID",
                table: "Guardians",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_SchoolID",
                table: "Managers",
                column: "SchoolID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_TenantID",
                table: "Managers",
                column: "TenantID",
                unique: true,
                filter: "[TenantID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_UserID",
                table: "Managers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_ClassID",
                table: "MonthlyGrades",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_GradeTypeID",
                table: "MonthlyGrades",
                column: "GradeTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_MonthID",
                table: "MonthlyGrades",
                column: "MonthID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_StudentID_SubjectID_MonthID_GradeTypeID",
                table: "MonthlyGrades",
                columns: new[] { "StudentID", "SubjectID", "MonthID", "GradeTypeID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_SubjectID",
                table: "MonthlyGrades",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_TermID",
                table: "MonthlyGrades",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyGrades_YearID",
                table: "MonthlyGrades",
                column: "YearID");

            migrationBuilder.CreateIndex(
                name: "IX_Salarys_TeacherID",
                table: "Salarys",
                column: "TeacherID");

            migrationBuilder.CreateIndex(
                name: "IX_Stages_YearID",
                table: "Stages",
                column: "YearID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassFees_FeeClassID",
                table: "StudentClassFees",
                column: "FeeClassID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClassFees_StudentID",
                table: "StudentClassFees",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_DivisionID",
                table: "Students",
                column: "DivisionID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_GuardianID",
                table: "Students",
                column: "GuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_UserID",
                table: "Students",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ManagerID",
                table: "Teachers",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserID",
                table: "Teachers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_ClassID",
                table: "TermlyGrades",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_StudentID_TermID_SubjectID_ClassID",
                table: "TermlyGrades",
                columns: new[] { "StudentID", "TermID", "SubjectID", "ClassID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_SubjectID",
                table: "TermlyGrades",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_TermID",
                table: "TermlyGrades",
                column: "TermID");

            migrationBuilder.CreateIndex(
                name: "IX_TermlyGrades_YearID",
                table: "TermlyGrades",
                column: "YearID");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_AccountStudentGuardianID",
                table: "Vouchers",
                column: "AccountStudentGuardianID");

            migrationBuilder.CreateIndex(
                name: "IX_Years_SchoolID",
                table: "Years",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_YearTermMonths_MonthID",
                table: "YearTermMonths",
                column: "MonthID");

            migrationBuilder.CreateIndex(
                name: "IX_YearTermMonths_TermID",
                table: "YearTermMonths",
                column: "TermID");
        }
    }
}
