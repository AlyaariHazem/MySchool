# MySchool Architecture Analysis

> **Scope:** Analysis of the `Backend` solution only. No code changes.  
> **Date:** June 2026  
> **Target:** Migration from monolith to microservices

---

## Executive Summary

MySchool is a **single-project ASP.NET Core 8 monolith** (`Backend.csproj`) paired with an **Angular** frontend (`MySchool/`). The backend uses a **multi-tenant split-database** model:

| Database | DbContext | Purpose |
|----------|-----------|---------|
| **Master** | `DatabaseContext` | Identity, tenants, permissions, subscriptions, registration |
| **Per-tenant** | `TenantDbContext` | All school business data (one SQL Server database per school) |

The architecture already separates platform concerns (master) from tenant concerns (per-school DB), which is a strong foundation for microservice extraction. However, **all domains share one deployable**, one `UnitOfWork` god-object, and extensive **cross-context coupling** (Identity user IDs stored in tenant entities, multi-DB orchestration in repositories).

---

## Solution Structure

```
MySchool/
├── Backend/                    ← ASP.NET Core 8 Web API (monolith)
│   ├── Backend.sln
│   ├── Backend.csproj
│   ├── Program.cs              ← Composition root (no Startup.cs)
│   ├── Controllers/
│   ├── Services/
│   ├── Repository/
│   ├── Interfaces/
│   ├── Models/
│   ├── Data/
│   ├── DTOS/
│   ├── Common/
│   ├── Authorization/
│   ├── UnitOfWork/
│   ├── Migrations/             ← Master DB migrations
│   └── Migrations/Tenant/      ← Tenant DB migrations
└── MySchool/                   ← Angular SPA (consumes Backend API)
```

### Projects and Dependencies

| Project | Type | Project References | NuGet Dependencies |
|---------|------|-------------------|-------------------|
| `Backend` | `net8.0` Web API | **None** (no class libraries) | AutoMapper 15.1.1, EF Core 8.0.10, Identity 8.0.10, JWT Bearer 8.0.10, Swashbuckle 6.4.0, HtmlSanitizer 9.0.892, Newtonsoft.Json |

**Key implication:** There are no separate shared-library projects. All cross-cutting code lives as folders inside the monolith (`Common/`, `Authorization/`, `DTOS/`, `Interfaces/`, etc.). Extraction will require creating new class libraries or duplicating contracts.

---

## DbContexts

### `DatabaseContext` (Master)

**File:** `Backend/Data/DatabaseContext.cs`  
**Base:** `IdentityDbContext<ApplicationUser>`  
**Connection:** `SqlAdminConnection` (appsettings)

| DbSet | Table / Purpose |
|-------|-----------------|
| `Tenants` | School tenant registry + connection strings |
| `UserTenants` | User ↔ tenant membership and per-tenant role |
| `Subscriptions` | Tenant subscription records |
| `TenantSettings` | Key-value settings per tenant |
| `RefreshTokens` | JWT refresh token storage |
| `RegistrationRequests` | Self-service school registration workflow |
| `RegistrationRequestAttachments` | Files attached to registration requests |
| `Permissions` | Fine-grained permission catalog |
| `RolePermissions` | Role ↔ permission mapping |
| *(Identity)* | `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, claims, logins, tokens |

### `TenantDbContext` (Per-Tenant)

**File:** `Backend/Data/TenantDbContext.cs`  
**Connection:** Resolved per-request via `TenantInfo.ConnectionString` (set by `TenantResolutionMiddleware`)  
**Scale:** 80+ `DbSet` properties covering all school business domains

**Supporting infrastructure:**

| File | Role |
|------|------|
| `TenantDbContextFactory.cs` | EF design-time factory for tenant migrations |
| `TenantSchemaBootstrapInterceptor.cs` | SQL Server tenant connection/bootstrap helpers |
| `TenantInfo.cs` | Request-scoped tenant ID + connection string |
| `TenantResolutionMiddleware.cs` | Resolves tenant from JWT `TenantId` claim |

---

## Controllers (54 files)

### Cross-cutting / Base

| Controller | Route | Purpose |
|------------|-------|---------|
| `GenericCrudController.cs` | — | Abstract generic CRUD base |
| `RolePermissionsController.cs` | `api/role-permissions` | Role ↔ permission admin |

### Identity / Auth

| Controller | Route | Purpose |
|------------|-------|---------|
| `Identity/AuthController.cs` | `api/auth` | Login, register, refresh, logout, tenant selection |
| `Identity/AuthController.RegistrationRequests.cs` | `api/auth` | Public schools, registration requests, approve/reject |

### School Domain (`Controllers/School/`)

| Controller | Route | Domain Area |
|------------|-------|-------------|
| `AccountsController` | `api/accounts` | Financial accounts |
| `AchievementRequestController` | `api/achievement-requests` | Achievement/award requests |
| `ActivityController` | `api/activities` | School activities |
| `AiController` | `api/ai` | AI assistant chat |
| `AnalyticsController` | `api/analytics` | Institutional analytics |
| `AttendanceController` | `api/attendance` | Daily attendance |
| `CentralPointsController` | `api/central-points` | Points/rewards ledger |
| `ClassesController` | `api/classes` | Class management |
| `ConcernController` | `api/concerns` | Complaints/suggestions |
| `CoursePlansController` | `api/courseplans` | Course planning |
| `CurriculmsController` | `api/curriculms` | Curricula |
| `DailyEvaluationController` | `api/daily-evaluations` | Daily teacher evaluations |
| `DashboardController` | `api/dashboard` | Dashboard aggregates |
| `DatabaseRestoreController` | `api/databaserestore` | SQL DB restore (admin) |
| `DivisionController` | `api/division` | Divisions within stages |
| `EmployeeController` | `api/employee` | Employee generic CRUD |
| `EmployeeRequestController` | `api/employee-requests` | Internal employee requests |
| `EmployeesController` | `api/employees` | Employee HR profiles |
| `ExamsController` | `api/exams` | Exams, sessions, results |
| `FeeClassController` | `api/feeclass` | Fee ↔ class mapping |
| `FeesController` | `api/fees` | Fee definitions |
| `FileController` | `api/file` | File upload/download |
| `GradeTypesController` | `api/gradetypes` | Grade type configuration |
| `GuardianController` | `api/guardian` | Parent/guardian records |
| `HomeworkController` | `api/homework` | Homework tasks/submissions |
| `ManagerController` | `api/manager` | School managers |
| `MeetingController` | `api/meetings` | Staff meetings |
| `MonthController` | `api/month` | Academic months |
| `MonthlyGradesController` | `api/monthlygrades` | Monthly grade entry |
| `NotificationsController` | `api/notifications` | In-app notifications |
| `OrganizationalPlanController` | `api/organizational-plans` | Strategic/operational plans |
| `RecruitmentController` | `api/recruitment` | Job postings, hiring pipeline |
| `ReportController` | `api/report` | Reports and templates |
| `SchoolController` | `api/school` | School profile |
| `StagesController` | `api/stages` | Academic stages |
| `StudentsController` | `api/students` | Student management |
| `SubjectController` | `api/subject` | Subjects |
| `SupervisorVisitController` | `api/supervisor-visits` | Supervisor classroom visits |
| `TeacherController` | `api/teacher` | Teachers |
| `TeacherFeedbackController` | `api/teacher-feedback` | Teacher performance feedback |
| `TeacherWorkspaceController` | `api/teacherworkspace` | Teacher dashboard summary |
| `TenantController` | `api/tenant` | Tenant (school) admin |
| `TenantSeedController` | `api/tenantseed` | Tenant demo/seed data |
| `TermController` | `api/term` | Academic terms |
| `TermlyGradeController` | `api/termlygrade` | Termly grade entry |
| `TimeCapsuleController` | `api/time-capsule` | Employee time capsule |
| `ViolationController` | `api/violations` | Student/staff violations |
| `VouchersController` | `api/vouchers` | Payment vouchers |
| `WeeklyScheduleController` | `api/weeklyschedule` | Weekly class schedules |
| `YearController` | `api/year` | Academic years |

---

## Repositories

### Pattern

- **Interface:** `Backend/Interfaces/I*Repository.cs` (62 files)
- **Implementation:** `Backend/Repository/` and `Backend/Repository/School/`
- **Aggregation:** `UnitOfWork` constructs all 40+ repositories and exposes them as properties
- **DI:** `ServiceCollectionExtensions.AddApplicationServices()` registers a subset; many repos are only accessed via `IUnitOfWork`

### Core Repositories

| Interface | Implementation | Primary DbContext |
|-----------|----------------|-------------------|
| `IRepository` / `IgenericRepository` | `Repository.cs` | Generic |
| `IGenericCrudRepository` | `GenericCrudRepository.cs` | Tenant |
| `IUserRepository` | `UsersRepository.cs` | **Master** (via `UserManager`) |
| `ITenantRepository` | `TenantRepository.cs` | **Master** |
| `ISchoolRepository` | `SchoolRepository.cs` | **Both** |
| `IManagerRepository` | `ManagerRepository.cs` | **Both** |
| `IDashboardRepository` | `DashboardRepository.cs` | **Both** |
| `IStudentRepository` | `StudentRepository.cs` | Tenant (+ Identity via `IUserRepository`) |
| `IGuardianRepository` | `GuardianRepository.cs` | Tenant (+ Identity) |
| `ITeacherRepository` | `TeacherRepositroy.cs` | Tenant (+ Identity) |
| `IEmployeeRepository` | `EmployeeRepository.cs` | Tenant (+ Identity, Students, Guardians) |
| `IClassesRepository` | `ClassesRepository.cs` | Tenant |
| `IStagesRepository` | `StagesRepository.cs` | Tenant |
| `IDivisionRepository` | `DivisionRepository.cs` | Tenant |
| `ISubjectsRepository` | `SubjectRepository.cs` | Tenant |
| `IYearRepository` | `YearRepository.cs` | Tenant |
| `ITermRepository` | `TermRepository.cs` | Tenant |
| `IMonthRepository` | `MonthRepository.cs` | Tenant |
| `ICurriculumRepository` | `CurriculumRepository.cs` | Tenant |
| `ICoursePlanRepository` | `CoursePlanRepository.cs` | Tenant |
| `IWeeklyScheduleRepository` | `WeeklyScheduleRepository.cs` | Tenant |
| `IAttendanceRepository` | `AttendanceRepository.cs` | Tenant |
| `IExamRepository` | `ExamRepository.cs` | Tenant |
| `IHomeworkRepository` | `HomeworkRepository.cs` | Tenant |
| `IGradeTypesRepository` | `GradeTypesRepository.cs` | Tenant |
| `IMonthlyGradeRepository` | `MonthlyGradeRepository.cs` | Tenant |
| `ITermlyGradeRepository` | `TermlyGradeRepository.cs` | Tenant |
| `IFeesRepository` | `FeesRepository.cs` | Tenant |
| `IFeeClassRepository` | `FeeClassRepostory.cs` | Tenant |
| `IStudentClassFeeRepository` | `StudentClassFeeRepository.cs` | Tenant |
| `IAccountRepository` | `AccountRepository.cs` | Tenant |
| `IVoucherRepository` | `VoucherRepostory.cs` | Tenant |
| `IAccountStudentGuardianRepository` | `AccountStudentGuardianRepository.cs` | Tenant |
| `IAttachmentRepository` | `AttachementsRepository.cs` | Tenant |
| `IReportRepository` | `ReportRepository.cs` | Tenant |
| `INotificationRepository` | `NotificationRepository.cs` | Tenant |
| `IAnalyticsRepository` | `AnalyticsRepository.cs` | Tenant |
| `ICentralPointsRepository` | `CentralPointsRepository.cs` | Tenant |
| `IActivityRepository` | `ActivityRepository.cs` | Tenant |
| `IAchievementRequestRepository` | `AchievementRequestRepository.cs` | Tenant |
| `IConcernRepository` | `ConcernRepository.cs` | Tenant |
| `IViolationRepository` | `ViolationRepository.cs` | Tenant |
| `IMeetingRepository` | `MeetingRepository.cs` | Tenant |
| `IEmployeeRequestRepository` | `EmployeeRequestRepository.cs` | Tenant |
| `ISupervisorVisitRepository` | `SupervisorVisitRepository.cs` | Tenant |
| `ITeacherFeedbackRepository` | `TeacherFeedbackRepository.cs` | Tenant |
| `IOrganizationalPlanRepository` | `OrganizationalPlanRepository.cs` | Tenant |

### Unit of Work

**File:** `Backend/UnitOfWork/UnitOfWork.cs`

`UnitOfWork` is the central coupling point. It injects:

- `TenantDbContext` and `DatabaseContext`
- `UserManager<ApplicationUser>`
- `IEmployeeYearAssignmentService`, `IAuditTrailService`, `HtmlSanitizationService`, `mangeFilesService`, `IApiBaseUrlProvider`

…and wires repository dependency chains (e.g., `Students` depends on `Guardians`, `Users`, `Years`; `Managers` depends on both DbContexts plus `Tenants` and `UserManager`).

---

## Services

### With Interface (`Interfaces/` ↔ `Services/`)

| Interface | Implementation | Notes |
|-----------|----------------|-------|
| `IAnalyticsService` | `AnalyticsService` | Cross-domain analytics |
| `IAuditTrailService` | `AuditTrailService` | Writes `AuditLog` in tenant DB |
| `IDailyEvaluationService` | `DailyEvaluationService` | Teacher daily evaluations |
| `IEmployeeProfileService` | `EmployeeProfileService` | HR employee profiles |
| `IEmployeeYearAssignmentService` | `EmployeeYearAssignmentService` | Employee ↔ year assignments |
| `IPermissionClaimService` | `PermissionClaimService` | Builds JWT permission claims |
| `IRecruitmentService` | `RecruitmentService` | Hiring pipeline |
| `IRolePermissionAdminService` | `RolePermissionAdminService` | Role permission admin |
| `ISchoolRoleResolver` | `SchoolRoleResolver` | Resolves school-side roles |
| `ITenantMembershipService` | `TenantMembershipService` | User ↔ tenant membership |
| `ITimeCapsuleService` | `TimeCapsuleService` | Employee time capsule |
| `IApiBaseUrlProvider` | `ApiBaseUrlProvider` | Upload URL construction |
| `IAutomaticWeeklyScheduleService` | `AutomaticWeeklyScheduleService` | Auto-generate schedules |

### Concrete Services (no dedicated interface)

| Service | Purpose |
|---------|---------|
| `StudentManagementService` | **Orchestrates** student + guardian + user + account + fees in one transaction |
| `mangeFilesService` | File upload/storage (`wwwroot/uploads`) |
| `HtmlSanitizationService` | HTML sanitization for reports/notifications |
| `TenantProvisioningService` | Creates tenant DB, runs migrations, registers in master |
| `TenantDatabaseFixService` | Tenant DB schema fixes |
| `TenantDemoDataSeeder` | Demo data seeding |
| `SqlRestoreService` | SQL backup restore |
| `PermissionSeeder` | Static permission seeding at startup |

### AI Services (`Services/Ai/`)

| Service | Purpose |
|---------|---------|
| `OpenAiChatCompletionService` | OpenAI HTTP client |
| `SchoolAiAssistantService` | AI chat orchestration |
| `SchoolAiToolsService` | Tool-calling against school APIs |
| `SchoolAiToolDefinitions` | Static tool schema definitions |
| `RegistrationReportMerger` | Registration report merge helper |

---

## Entities (159 model files)

### Master DB (`Models/Master/` + root)

| Entity | Service Owner (target) |
|--------|------------------------|
| `ApplicationUser` | Identity |
| `RefreshToken` | Identity |
| `Permission`, `RolePermission` | Identity |
| `Tenant` | Tenant |
| `UserTenant` | Tenant |
| `Subscription`, `TenantSettings` | Tenant |
| `RegistrationRequest`, `RegistrationRequestAttachment` | Tenant |

### Tenant DB — grouped by target microservice

See [bounded-contexts.md](./bounded-contexts.md) for full entity-to-service mapping.

---

## Authentication and Authorization

### Authentication Stack

| Component | File | Role |
|-----------|------|------|
| Identity setup | `Program.cs` | `AddIdentity<ApplicationUser, IdentityRole>` with EF stores on `DatabaseContext` |
| JWT Bearer | `Program.cs` | Symmetric key validation; `RoleClaimType` = `ClaimTypes.Role` |
| Default policy | `Program.cs` | All controllers require authentication unless `[AllowAnonymous]` |
| Auth API | `Controllers/Identity/AuthController*.cs` | Login, register, refresh, tenant selection, registration workflow |
| Refresh tokens | `Models/RefreshToken.cs` | Stored in master DB |
| Tenant claim | JWT `TenantId` claim | Set after `POST /api/auth/select-tenant` |

### Authorization Stack

| Component | File | Role |
|-----------|------|------|
| `[HasPermission]` attribute | `Authorization/HasPermissionAttribute.cs` | Policy-based fine-grained permissions |
| `PermissionAuthorizationHandler` | `Authorization/PermissionAuthorizationHandler.cs` | Checks `permission` claims; ADMIN bypass |
| `PagePermissionNames` | `Common/PagePermissionNames.cs` | All permission constants; registered as policies in `Program.cs` |
| `PermissionClaimService` | `Services/PermissionClaimService.cs` | Embeds permissions in JWT at login |
| `PlatformAdminHelper` | `Common/PlatformAdminHelper.cs` | Platform admin bypass for cross-tenant operations |
| `SchoolUserRoleKeys` | `Common/SchoolUserRoleKeys.cs` | Role key constants |

### Tenant Resolution

| Component | File | Role |
|-----------|------|------|
| `TenantResolutionMiddleware` | `Data/TenantResolutionMiddleware.cs` | Reads `TenantId` from JWT → loads connection string from master `Tenants` table |
| `TenantInfo` | `Data/TenantInfo.cs` | Scoped per-request tenant context |
| Platform admin mode | Middleware + repos | Allows master-only routes without tenant connection for cross-tenant catalog |

### Seeded Roles

`ADMIN`, `GUARDIAN`, `STUDENT`, `TEACHER`, `MANAGER` — seeded at startup in `Program.cs`.

---

## Shared Libraries (In-Process Folders)

These are **not separate projects** today but will need extraction as shared NuGet packages or a `MySchool.Shared` library:

| Folder | Contents | Migration Target |
|--------|----------|------------------|
| `Common/` | `PagedResult`, `FilterRequest`, `Result`, `PagePermissionNames`, `PlatformAdminHelper`, `StudentIdGenerator`, `YearSeeding`, pagination/filtering | Shared contracts library |
| `Authorization/` | Permission attribute, handler, requirement | Identity service + shared auth library |
| `DTOS/` | 153+ DTO files across all domains | Per-service contracts + shared primitives |
| `Interfaces/` | Repository and service interfaces | Per-service contracts |
| `Extensions/` | `ServiceCollectionExtensions` DI bootstrap | Per-service `Program.cs` |
| `Configuration/` | `SqlRestoreOptions`, `OpenAiOptions` | Per-service config |
| `MappingConfig.cs` | AutoMapper profiles (all domains) | Split per service |

---

## Frontend Coupling

The Angular app (`MySchool/`) calls the monolith API at a single base URL. Key integration points:

- JWT stored client-side; sent as `Bearer` token
- `TenantId` claim required for school-scoped routes
- File uploads served from `wwwroot/uploads/` via static files middleware
- CORS configured for `http://localhost:4200` (dev)

A future API Gateway or BFF will need to preserve these contracts during migration.

---

## Architectural Strengths (Migration Enablers)

1. **Master vs tenant DB split** already exists — natural boundary for Identity/Tenant vs school services
2. **Repository pattern** with interfaces — enables swapping in-process calls for HTTP/gRPC clients
3. **Permission-based authorization** — can be centralized in Identity service with token claims
4. **AI module** (`Services/Ai/`) is already somewhat isolated
5. **Tenant migrations** are separate from master migrations

## Architectural Risks (Migration Blockers)

1. **`UnitOfWork` god-object** — all repos constructed together; hides service boundaries
2. **Cross-DB repositories** — `ManagerRepository`, `SchoolRepository`, `DashboardRepository` open multiple tenant DBs via master catalog
3. **Identity coupling in tenant entities** — `Student.UserID`, `Teacher.UserID`, `Guardian.UserID`, `Manager.UserID` reference `AspNetUsers.Id`
4. **Distributed transactions** — `StudentManagementService` creates users + students + accounts + fees in one SQL transaction spanning logical services
5. **Shared `TenantDbContext`** — 80+ tables in one context; no physical DB separation within tenant
6. **AutoMapper monolith** — single `MappingConfig` maps all domains
7. **Platform admin cross-tenant aggregation** — dashboard and manager catalog iterate all tenant databases
