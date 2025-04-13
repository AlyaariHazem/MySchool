// using System;
// using Microsoft.EntityFrameworkCore.Migrations;

// #nullable disable

// #pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

// namespace Backend.Migrations
// {
//     /// <inheritdoc />
//     public partial class EditFOreignKeyFOrFeeClass : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.CreateTable(
//                 name: "AspNetRoles",
//                 columns: table => new
//                 {
//                     Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
//                     NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
//                     ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AspNetRoles", x => x.Id);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "AspNetUsers",
//                 columns: table => new
//                 {
//                     Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
//                     NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
//                     Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
//                     NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
//                     EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
//                     PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
//                     TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
//                     LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
//                     LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
//                     AccessFailedCount = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AspNetUsers", x => x.Id);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Fees",
//                 columns: table => new
//                 {
//                     FeeID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     FeeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FeeNameAlis = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     State = table.Column<bool>(type: "bit", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Fees", x => x.FeeID);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Schools",
//                 columns: table => new
//                 {
//                     SchoolID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     SchoolNameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     SchoolVison = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     SchoolMission = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     SchoolGoal = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     City = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     SchoolPhone = table.Column<int>(type: "int", nullable: false),
//                     Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     SchoolType = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     fax = table.Column<int>(type: "int", nullable: true),
//                     zone = table.Column<string>(type: "nvarchar(max)", nullable: true)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Schools", x => x.SchoolID);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "TypeAccounts",
//                 columns: table => new
//                 {
//                     TypeAccountID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     TypeAccountName = table.Column<string>(type: "nvarchar(max)", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_TypeAccounts", x => x.TypeAccountID);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "AspNetRoleClaims",
//                 columns: table => new
//                 {
//                     Id = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
//                     table.ForeignKey(
//                         name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
//                         column: x => x.RoleId,
//                         principalTable: "AspNetRoles",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "AspNetUserClaims",
//                 columns: table => new
//                 {
//                     Id = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
//                     table.ForeignKey(
//                         name: "FK_AspNetUserClaims_AspNetUsers_UserId",
//                         column: x => x.UserId,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "AspNetUserLogins",
//                 columns: table => new
//                 {
//                     LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
//                     table.ForeignKey(
//                         name: "FK_AspNetUserLogins_AspNetUsers_UserId",
//                         column: x => x.UserId,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "AspNetUserRoles",
//                 columns: table => new
//                 {
//                     UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
//                     table.ForeignKey(
//                         name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
//                         column: x => x.RoleId,
//                         principalTable: "AspNetRoles",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                     table.ForeignKey(
//                         name: "FK_AspNetUserRoles_AspNetUsers_UserId",
//                         column: x => x.UserId,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "AspNetUserTokens",
//                 columns: table => new
//                 {
//                     UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
//                     table.ForeignKey(
//                         name: "FK_AspNetUserTokens_AspNetUsers_UserId",
//                         column: x => x.UserId,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Guardians",
//                 columns: table => new
//                 {
//                     GuardianID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     UserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Guardians", x => x.GuardianID);
//                     table.ForeignKey(
//                         name: "FK_Guardians_AspNetUsers_UserID",
//                         column: x => x.UserID,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Managers",
//                 columns: table => new
//                 {
//                     ManagerID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     FullName_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FullName_MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FullName_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     SchoolID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Managers", x => x.ManagerID);
//                     table.ForeignKey(
//                         name: "FK_Managers_AspNetUsers_UserID",
//                         column: x => x.UserID,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                     table.ForeignKey(
//                         name: "FK_Managers_Schools_SchoolID",
//                         column: x => x.SchoolID,
//                         principalTable: "Schools",
//                         principalColumn: "SchoolID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Years",
//                 columns: table => new
//                 {
//                     YearID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     YearDateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     YearDateEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
//                     HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     Active = table.Column<bool>(type: "bit", nullable: false),
//                     SchoolID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Years", x => x.YearID);
//                     table.ForeignKey(
//                         name: "FK_Years_Schools_SchoolID",
//                         column: x => x.SchoolID,
//                         principalTable: "Schools",
//                         principalColumn: "SchoolID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Accounts",
//                 columns: table => new
//                 {
//                     AccountID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     State = table.Column<bool>(type: "bit", nullable: false),
//                     Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     OpenBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
//                     TypeOpenBalance = table.Column<bool>(type: "bit", nullable: false),
//                     HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     TypeAccountID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Accounts", x => x.AccountID);
//                     table.ForeignKey(
//                         name: "FK_Accounts_TypeAccounts_TypeAccountID",
//                         column: x => x.TypeAccountID,
//                         principalTable: "TypeAccounts",
//                         principalColumn: "TypeAccountID",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Teachers",
//                 columns: table => new
//                 {
//                     TeacherID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     FullName_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FullName_MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FullName_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
//                     ManagerID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Teachers", x => x.TeacherID);
//                     table.ForeignKey(
//                         name: "FK_Teachers_AspNetUsers_UserID",
//                         column: x => x.UserID,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                     table.ForeignKey(
//                         name: "FK_Teachers_Managers_ManagerID",
//                         column: x => x.ManagerID,
//                         principalTable: "Managers",
//                         principalColumn: "ManagerID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Stages",
//                 columns: table => new
//                 {
//                     StageID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     StageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     Active = table.Column<bool>(type: "bit", nullable: false),
//                     HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     YearID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Stages", x => x.StageID);
//                     table.ForeignKey(
//                         name: "FK_Stages_Years_YearID",
//                         column: x => x.YearID,
//                         principalTable: "Years",
//                         principalColumn: "YearID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Salarys",
//                 columns: table => new
//                 {
//                     SalaryID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     SalaryMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     SalaryHireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     SalaryAmount = table.Column<int>(type: "int", nullable: false),
//                     TeacherID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Salarys", x => x.SalaryID);
//                     table.ForeignKey(
//                         name: "FK_Salarys_Teachers_TeacherID",
//                         column: x => x.TeacherID,
//                         principalTable: "Teachers",
//                         principalColumn: "TeacherID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Classes",
//                 columns: table => new
//                 {
//                     ClassID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     ClassName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     ClassYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     StageID = table.Column<int>(type: "int", nullable: false),
//                     State = table.Column<bool>(type: "bit", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Classes", x => x.ClassID);
//                     table.ForeignKey(
//                         name: "FK_Classes_Stages_StageID",
//                         column: x => x.StageID,
//                         principalTable: "Stages",
//                         principalColumn: "StageID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Divisions",
//                 columns: table => new
//                 {
//                     DivisionID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     DivisionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     State = table.Column<bool>(type: "bit", nullable: false),
//                     ClassID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Divisions", x => x.DivisionID);
//                     table.ForeignKey(
//                         name: "FK_Divisions_Classes_ClassID",
//                         column: x => x.ClassID,
//                         principalTable: "Classes",
//                         principalColumn: "ClassID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "FeeClass",
//                 columns: table => new
//                 {
//                     FeeClassID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     ClassID = table.Column<int>(type: "int", nullable: false),
//                     FeeID = table.Column<int>(type: "int", nullable: false),
//                     Amount = table.Column<double>(type: "float", nullable: true),
//                     Mandatory = table.Column<bool>(type: "bit", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_FeeClass", x => x.FeeClassID);
//                     table.ForeignKey(
//                         name: "FK_FeeClass_Classes_ClassID",
//                         column: x => x.ClassID,
//                         principalTable: "Classes",
//                         principalColumn: "ClassID",
//                         onDelete: ReferentialAction.Restrict);
//                     table.ForeignKey(
//                         name: "FK_FeeClass_Fees_FeeID",
//                         column: x => x.FeeID,
//                         principalTable: "Fees",
//                         principalColumn: "FeeID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Subjects",
//                 columns: table => new
//                 {
//                     SubjectID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     ClassID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Subjects", x => x.SubjectID);
//                     table.ForeignKey(
//                         name: "FK_Subjects_Classes_ClassID",
//                         column: x => x.ClassID,
//                         principalTable: "Classes",
//                         principalColumn: "ClassID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Students",
//                 columns: table => new
//                 {
//                     StudentID = table.Column<int>(type: "int", nullable: false),
//                     FullName_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FullName_MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FullName_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     FullNameAlis_FirstNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     FullNameAlis_MiddleNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     FullNameAlis_LastNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     PlaceBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     DivisionID = table.Column<int>(type: "int", nullable: false),
//                     GuardianID = table.Column<int>(type: "int", nullable: false, defaultValue: 1012),
//                     UserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Students", x => x.StudentID);
//                     table.ForeignKey(
//                         name: "FK_Students_AspNetUsers_UserID",
//                         column: x => x.UserID,
//                         principalTable: "AspNetUsers",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                     table.ForeignKey(
//                         name: "FK_Students_Divisions_DivisionID",
//                         column: x => x.DivisionID,
//                         principalTable: "Divisions",
//                         principalColumn: "DivisionID",
//                         onDelete: ReferentialAction.Restrict);
//                     table.ForeignKey(
//                         name: "FK_Students_Guardians_GuardianID",
//                         column: x => x.GuardianID,
//                         principalTable: "Guardians",
//                         principalColumn: "GuardianID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "AccountStudentGuardians",
//                 columns: table => new
//                 {
//                     AccountStudentGuardianID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     AccountID = table.Column<int>(type: "int", nullable: false),
//                     GuardianID = table.Column<int>(type: "int", nullable: false),
//                     StudentID = table.Column<int>(type: "int", nullable: false),
//                     Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_AccountStudentGuardians", x => x.AccountStudentGuardianID);
//                     table.ForeignKey(
//                         name: "FK_AccountStudentGuardians_Accounts_AccountID",
//                         column: x => x.AccountID,
//                         principalTable: "Accounts",
//                         principalColumn: "AccountID",
//                         onDelete: ReferentialAction.Restrict);
//                     table.ForeignKey(
//                         name: "FK_AccountStudentGuardians_Guardians_GuardianID",
//                         column: x => x.GuardianID,
//                         principalTable: "Guardians",
//                         principalColumn: "GuardianID",
//                         onDelete: ReferentialAction.Restrict);
//                     table.ForeignKey(
//                         name: "FK_AccountStudentGuardians_Students_StudentID",
//                         column: x => x.StudentID,
//                         principalTable: "Students",
//                         principalColumn: "StudentID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "StudentClassFees",
//                 columns: table => new
//                 {
//                     StudentClassFeesID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     StudentID = table.Column<int>(type: "int", nullable: false),
//                     FeeClassID = table.Column<int>(type: "int", nullable: false),
//                     AmountDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
//                     NoteDiscount = table.Column<string>(type: "nvarchar(max)", nullable: true)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_StudentClassFees", x => x.StudentClassFeesID);
//                     table.ForeignKey(
//                         name: "FK_StudentClassFees_FeeClass_FeeClassID",
//                         column: x => x.FeeClassID,
//                         principalTable: "FeeClass",
//                         principalColumn: "FeeClassID",
//                         onDelete: ReferentialAction.Restrict);
//                     table.ForeignKey(
//                         name: "FK_StudentClassFees_Students_StudentID",
//                         column: x => x.StudentID,
//                         principalTable: "Students",
//                         principalColumn: "StudentID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "SubjectStudents",
//                 columns: table => new
//                 {
//                     SubjectID = table.Column<int>(type: "int", nullable: false),
//                     StudentID = table.Column<int>(type: "int", nullable: false),
//                     Grade = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_SubjectStudents", x => new { x.SubjectID, x.StudentID });
//                     table.ForeignKey(
//                         name: "FK_SubjectStudents_Students_StudentID",
//                         column: x => x.StudentID,
//                         principalTable: "Students",
//                         principalColumn: "StudentID",
//                         onDelete: ReferentialAction.Restrict);
//                     table.ForeignKey(
//                         name: "FK_SubjectStudents_Subjects_SubjectID",
//                         column: x => x.SubjectID,
//                         principalTable: "Subjects",
//                         principalColumn: "SubjectID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "TeacherStudents",
//                 columns: table => new
//                 {
//                     StudentID = table.Column<int>(type: "int", nullable: false),
//                     TeacherID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_TeacherStudents", x => new { x.StudentID, x.TeacherID });
//                     table.ForeignKey(
//                         name: "FK_TeacherStudents_Students_StudentID",
//                         column: x => x.StudentID,
//                         principalTable: "Students",
//                         principalColumn: "StudentID",
//                         onDelete: ReferentialAction.Restrict);
//                     table.ForeignKey(
//                         name: "FK_TeacherStudents_Teachers_TeacherID",
//                         column: x => x.TeacherID,
//                         principalTable: "Teachers",
//                         principalColumn: "TeacherID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "TeacherSubjectStudent",
//                 columns: table => new
//                 {
//                     TeacherID = table.Column<int>(type: "int", nullable: false),
//                     StudentID = table.Column<int>(type: "int", nullable: false),
//                     SubjectID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_TeacherSubjectStudent", x => new { x.TeacherID, x.StudentID, x.SubjectID });
//                     table.ForeignKey(
//                         name: "FK_TeacherSubjectStudent_Students_StudentID",
//                         column: x => x.StudentID,
//                         principalTable: "Students",
//                         principalColumn: "StudentID",
//                         onDelete: ReferentialAction.Cascade);
//                     table.ForeignKey(
//                         name: "FK_TeacherSubjectStudent_Subjects_SubjectID",
//                         column: x => x.SubjectID,
//                         principalTable: "Subjects",
//                         principalColumn: "SubjectID",
//                         onDelete: ReferentialAction.Cascade);
//                     table.ForeignKey(
//                         name: "FK_TeacherSubjectStudent_Teachers_TeacherID",
//                         column: x => x.TeacherID,
//                         principalTable: "Teachers",
//                         principalColumn: "TeacherID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "vouchers",
//                 columns: table => new
//                 {
//                     VoucherID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     Receipt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
//                     HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
//                     Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     PayBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     AccountStudentGuardianID = table.Column<int>(type: "int", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_vouchers", x => x.VoucherID);
//                     table.ForeignKey(
//                         name: "FK_vouchers_AccountStudentGuardians_AccountStudentGuardianID",
//                         column: x => x.AccountStudentGuardianID,
//                         principalTable: "AccountStudentGuardians",
//                         principalColumn: "AccountStudentGuardianID",
//                         onDelete: ReferentialAction.Restrict);
//                 });

//             migrationBuilder.CreateTable(
//                 name: "Attachments",
//                 columns: table => new
//                 {
//                     AttachmentID = table.Column<int>(type: "int", nullable: false)
//                         .Annotation("SqlServer:Identity", "1, 1"),
//                     AttachmentURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                     Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
//                     StudentID = table.Column<int>(type: "int", nullable: true),
//                     VoucherID = table.Column<int>(type: "int", nullable: true)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_Attachments", x => x.AttachmentID);
//                     table.ForeignKey(
//                         name: "FK_Attachments_Students_StudentID",
//                         column: x => x.StudentID,
//                         principalTable: "Students",
//                         principalColumn: "StudentID",
//                         onDelete: ReferentialAction.Cascade);
//                     table.ForeignKey(
//                         name: "FK_Attachments_vouchers_VoucherID",
//                         column: x => x.VoucherID,
//                         principalTable: "vouchers",
//                         principalColumn: "VoucherID",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.InsertData(
//                 table: "TypeAccounts",
//                 columns: new[] { "TypeAccountID", "TypeAccountName" },
//                 values: new object[,]
//                 {
//                     { 1, "Guardain" },
//                     { 2, "School" },
//                     { 3, "Branches" },
//                     { 4, "Funds" },
//                     { 5, "Employees" },
//                     { 6, "Banks" }
//                 });

//             migrationBuilder.CreateIndex(
//                 name: "IX_Accounts_TypeAccountID",
//                 table: "Accounts",
//                 column: "TypeAccountID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_AccountStudentGuardians_AccountID",
//                 table: "AccountStudentGuardians",
//                 column: "AccountID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_AccountStudentGuardians_GuardianID",
//                 table: "AccountStudentGuardians",
//                 column: "GuardianID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_AccountStudentGuardians_StudentID",
//                 table: "AccountStudentGuardians",
//                 column: "StudentID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_AspNetRoleClaims_RoleId",
//                 table: "AspNetRoleClaims",
//                 column: "RoleId");

//             migrationBuilder.CreateIndex(
//                 name: "RoleNameIndex",
//                 table: "AspNetRoles",
//                 column: "NormalizedName",
//                 unique: true,
//                 filter: "[NormalizedName] IS NOT NULL");

//             migrationBuilder.CreateIndex(
//                 name: "IX_AspNetUserClaims_UserId",
//                 table: "AspNetUserClaims",
//                 column: "UserId");

//             migrationBuilder.CreateIndex(
//                 name: "IX_AspNetUserLogins_UserId",
//                 table: "AspNetUserLogins",
//                 column: "UserId");

//             migrationBuilder.CreateIndex(
//                 name: "IX_AspNetUserRoles_RoleId",
//                 table: "AspNetUserRoles",
//                 column: "RoleId");

//             migrationBuilder.CreateIndex(
//                 name: "EmailIndex",
//                 table: "AspNetUsers",
//                 column: "NormalizedEmail");

//             migrationBuilder.CreateIndex(
//                 name: "UserNameIndex",
//                 table: "AspNetUsers",
//                 column: "NormalizedUserName",
//                 unique: true,
//                 filter: "[NormalizedUserName] IS NOT NULL");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Attachments_StudentID",
//                 table: "Attachments",
//                 column: "StudentID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Attachments_VoucherID",
//                 table: "Attachments",
//                 column: "VoucherID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Classes_StageID",
//                 table: "Classes",
//                 column: "StageID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Divisions_ClassID",
//                 table: "Divisions",
//                 column: "ClassID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_FeeClass_ClassID",
//                 table: "FeeClass",
//                 column: "ClassID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_FeeClass_FeeID",
//                 table: "FeeClass",
//                 column: "FeeID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Guardians_UserID",
//                 table: "Guardians",
//                 column: "UserID",
//                 unique: true);

//             migrationBuilder.CreateIndex(
//                 name: "IX_Managers_SchoolID",
//                 table: "Managers",
//                 column: "SchoolID",
//                 unique: true);

//             migrationBuilder.CreateIndex(
//                 name: "IX_Managers_UserID",
//                 table: "Managers",
//                 column: "UserID",
//                 unique: true);

//             migrationBuilder.CreateIndex(
//                 name: "IX_Salarys_TeacherID",
//                 table: "Salarys",
//                 column: "TeacherID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Stages_YearID",
//                 table: "Stages",
//                 column: "YearID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_StudentClassFees_FeeClassID",
//                 table: "StudentClassFees",
//                 column: "FeeClassID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_StudentClassFees_StudentID",
//                 table: "StudentClassFees",
//                 column: "StudentID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Students_DivisionID",
//                 table: "Students",
//                 column: "DivisionID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Students_GuardianID",
//                 table: "Students",
//                 column: "GuardianID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Students_UserID",
//                 table: "Students",
//                 column: "UserID",
//                 unique: true);

//             migrationBuilder.CreateIndex(
//                 name: "IX_Subjects_ClassID",
//                 table: "Subjects",
//                 column: "ClassID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_SubjectStudents_StudentID",
//                 table: "SubjectStudents",
//                 column: "StudentID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Teachers_ManagerID",
//                 table: "Teachers",
//                 column: "ManagerID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Teachers_UserID",
//                 table: "Teachers",
//                 column: "UserID",
//                 unique: true);

//             migrationBuilder.CreateIndex(
//                 name: "IX_TeacherStudents_TeacherID",
//                 table: "TeacherStudents",
//                 column: "TeacherID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_TeacherSubjectStudent_StudentID",
//                 table: "TeacherSubjectStudent",
//                 column: "StudentID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_TeacherSubjectStudent_SubjectID",
//                 table: "TeacherSubjectStudent",
//                 column: "SubjectID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_vouchers_AccountStudentGuardianID",
//                 table: "vouchers",
//                 column: "AccountStudentGuardianID");

//             migrationBuilder.CreateIndex(
//                 name: "IX_Years_SchoolID",
//                 table: "Years",
//                 column: "SchoolID");
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropTable(
//                 name: "AspNetRoleClaims");

//             migrationBuilder.DropTable(
//                 name: "AspNetUserClaims");

//             migrationBuilder.DropTable(
//                 name: "AspNetUserLogins");

//             migrationBuilder.DropTable(
//                 name: "AspNetUserRoles");

//             migrationBuilder.DropTable(
//                 name: "AspNetUserTokens");

//             migrationBuilder.DropTable(
//                 name: "Attachments");

//             migrationBuilder.DropTable(
//                 name: "Salarys");

//             migrationBuilder.DropTable(
//                 name: "StudentClassFees");

//             migrationBuilder.DropTable(
//                 name: "SubjectStudents");

//             migrationBuilder.DropTable(
//                 name: "TeacherStudents");

//             migrationBuilder.DropTable(
//                 name: "TeacherSubjectStudent");

//             migrationBuilder.DropTable(
//                 name: "AspNetRoles");

//             migrationBuilder.DropTable(
//                 name: "vouchers");

//             migrationBuilder.DropTable(
//                 name: "FeeClass");

//             migrationBuilder.DropTable(
//                 name: "Subjects");

//             migrationBuilder.DropTable(
//                 name: "Teachers");

//             migrationBuilder.DropTable(
//                 name: "AccountStudentGuardians");

//             migrationBuilder.DropTable(
//                 name: "Fees");

//             migrationBuilder.DropTable(
//                 name: "Managers");

//             migrationBuilder.DropTable(
//                 name: "Accounts");

//             migrationBuilder.DropTable(
//                 name: "Students");

//             migrationBuilder.DropTable(
//                 name: "TypeAccounts");

//             migrationBuilder.DropTable(
//                 name: "Divisions");

//             migrationBuilder.DropTable(
//                 name: "Guardians");

//             migrationBuilder.DropTable(
//                 name: "Classes");

//             migrationBuilder.DropTable(
//                 name: "AspNetUsers");

//             migrationBuilder.DropTable(
//                 name: "Stages");

//             migrationBuilder.DropTable(
//                 name: "Years");

//             migrationBuilder.DropTable(
//                 name: "Schools");
//         }
//     }
// }
