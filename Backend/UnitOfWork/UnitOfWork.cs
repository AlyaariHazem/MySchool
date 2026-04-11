using System;
using System.Threading.Tasks;
using Backend.Repository;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Backend.Interfaces;
using Backend.Data;
using Backend.Repository.School.Classes;
using Backend.Repository.School;
using FirstProjectWithMVC.Repository.School;
using AutoMapper;
using Backend.Services;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity; // Assuming your DbContext is here
using Microsoft.Extensions.Logging;

public class UnitOfWork : IUnitOfWork
{
    private readonly TenantDbContext _tenantContext; // For tenant-specific data
    private readonly DatabaseContext _adminContext; // For master DB (Tenants, AspNetUsers, etc.)
    private readonly IMapper _mapper;
    private readonly mangeFilesService _mangeFilesService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StudentRepository> _studentRepositoryLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HtmlSanitizationService _htmlSanitizer;
    private readonly IAuditTrailService _auditTrail;
    private readonly TenantInfo _tenantInfo;
    private readonly IApiBaseUrlProvider _apiBaseUrl;

    public UnitOfWork(
        TenantDbContext tenantContext,
        DatabaseContext adminContext,
        IMapper mapper,
        mangeFilesService mangeFilesService,
        UserManager<ApplicationUser> userManager,
        ILogger<StudentRepository> studentRepositoryLogger,
        IHttpContextAccessor httpContextAccessor,
        HtmlSanitizationService htmlSanitizer,
        IAuditTrailService auditTrail,
        TenantInfo tenantInfo,
        IApiBaseUrlProvider apiBaseUrl)
    {
        _tenantContext = tenantContext;
        _adminContext = adminContext;
        _tenantInfo = tenantInfo;
        _mapper = mapper;
        _mangeFilesService = mangeFilesService;
        _userManager = userManager;
        _studentRepositoryLogger = studentRepositoryLogger;
        _httpContextAccessor = httpContextAccessor;
        _htmlSanitizer = htmlSanitizer;
        _auditTrail = auditTrail;
        _apiBaseUrl = apiBaseUrl;

        // Tenant-specific repositories use TenantDbContext
        // Initialize Users first since it's needed by Students, Teachers, and Employees
        Users = new UsersRepository(_userManager);
        Guardians = new GuardianRepository(_tenantContext, _mapper, Users);
        Subjects = new SubjectRepository(_tenantContext, _mapper);
        Students = new StudentRepository(_tenantContext, Guardians, _mangeFilesService, Users, _studentRepositoryLogger, _apiBaseUrl);
        Classes = new ClassesRepository(_tenantContext, _mapper);
        Divisions = new DivisionRepository(_tenantContext, _mapper);
        Stages = new StagesRepository(_tenantContext, _mapper);
        Accounts = new AccountRepository(_tenantContext, _mapper);
        FeeClasses = new FeeClassRepository(_tenantContext, _mapper);
        Fees = new FeesRepository(_tenantContext, _mapper);
        Years = new YearRepository(_tenantContext, _mapper);
        Schools = new SchoolRepository(_tenantContext, _adminContext, Years, _mapper, _httpContextAccessor, _tenantInfo, _apiBaseUrl);
        StudentClassFees = new StudentClassFeeRepository(_tenantContext, _mapper, _auditTrail);
        Vouchers = new VoucherRepository(_tenantContext, _mapper, _mangeFilesService);
        Attachments = new AttachmentsRepository(_tenantContext);
        Curriculums = new CurriculumRepository(_tenantContext, _mapper);
        CoursePlans = new CoursePlanRepository(_tenantContext, _mapper);
        Teachers = new TeacherRepository(_tenantContext, Users, _apiBaseUrl);
        GradeTypes = new GradeTypesRepository(_tenantContext, _mapper);
        Terms = new TermRepository(_tenantContext, _mapper);
        Months = new MonthRepository(_tenantContext, _mapper);
        Employees = new EmployeeRepository(_tenantContext, Users);
        AccountStudentGuardians = new AccountStudentGuardianRepository(_tenantContext, _mapper);
        Reports = new ReportRepository(_tenantContext, _htmlSanitizer, _apiBaseUrl);
        MonthlyGrades = new MonthlyGradeRepository(_tenantContext, _mapper, _auditTrail, _apiBaseUrl);
        TermlyGrades = new TermlyGradeRepository(_tenantContext, _mapper, _auditTrail, _apiBaseUrl);
        Dashboard = new DashboardRepository(_tenantContext, _adminContext, _tenantInfo, _httpContextAccessor);
        WeeklySchedules = new WeeklyScheduleRepository(_tenantContext, _mapper);
        Attendance = new AttendanceRepository(_tenantContext);
        Notifications = new NotificationRepository(_tenantContext, _htmlSanitizer);
        Exams = new ExamRepository(_tenantContext);
        Homework = new HomeworkRepository(_tenantContext);
        
        // Master DB repositories use DatabaseContext
        Tenants = new TenantRepository(_adminContext, _mapper);
        Managers = new ManagerRepository(_tenantContext, _adminContext, Users, Tenants, _userManager, _tenantInfo, _httpContextAccessor);
    }

    public ISubjectsRepository Subjects { get; private set; }
    public IStudentRepository Students { get; private set; }
    public IClassesRepository Classes { get; private set; }
    public IDivisionRepository Divisions { get; private set; }
    public IGuardianRepository Guardians { get; private set; }
    public IUserRepository Users { get; private set; }
    public IStagesRepository Stages { get; private set; }
    public IAccountRepository Accounts { get; private set; }
    public IFeeClassRepository FeeClasses { get; private set; }
    public IFeesRepository Fees { get; private set; }
    public IManagerRepository Managers { get; private set; }
    public ISchoolRepository Schools { get; private set; }
    public IStudentClassFeeRepository StudentClassFees { get; private set; }
    public ITenantRepository Tenants { get; private set; }
    public IYearRepository Years { get; private set; }
    public IVoucherRepository Vouchers { get; private set; }
    public IAttachmentRepository Attachments { get; private set; }
    public ICurriculumRepository Curriculums { get; private set; }
    public ICoursePlanRepository CoursePlans { get; private set; }
    public ITeacherRepository Teachers { get; private set; }
    public IGradeTypesRepository GradeTypes { get; private set; }
    public ITermRepository Terms { get; private set; }
    public IMonthRepository Months { get; private set; }
    public IEmployeeRepository Employees { get; private set; }
    public IAccountStudentGuardianRepository AccountStudentGuardians { get; private set; }
    public IReportRepository Reports { get; private set; }
    public IMonthlyGradeRepository MonthlyGrades { get; private set; }
    public ITermlyGradeRepository TermlyGrades { get; private set; }
    public IDashboardRepository Dashboard { get; private set; }
    public IWeeklyScheduleRepository WeeklySchedules { get; private set; }
    public IAttendanceRepository Attendance { get; private set; }
    public INotificationRepository Notifications { get; private set; }
    public IExamRepository Exams { get; private set; }
    public IHomeworkRepository Homework { get; private set; }

    public async Task<int> CompleteAsync()
    {
        // Save changes to tenant database
        return await _tenantContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _tenantContext?.Dispose();
        _adminContext?.Dispose();
    }
}
