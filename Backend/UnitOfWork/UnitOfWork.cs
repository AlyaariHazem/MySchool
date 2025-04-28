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
using Microsoft.AspNetCore.Identity; // Assuming your DbContext is here

public class UnitOfWork : IUnitOfWork
{
    private readonly DatabaseContext _context;
    private readonly IMapper _mapper;
    private readonly mangeFilesService _mangeFilesService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UnitOfWork(
        DatabaseContext context,
        IMapper mapper,
        mangeFilesService mangeFilesService,
        UserManager<ApplicationUser> userManager
    )
    {
        _context = context;
        _mapper = mapper;
        _mangeFilesService = mangeFilesService;
        _userManager = userManager;

        // أنشئ الريبو بالترتيب الذي يعتمد عليه بعضهم بعضًا
        Guardians = new GuardianRepository(_context, _mapper);
        Subjects = new SubjectRepository(_context, _mapper);
        Students = new StudentRepository(_context, Guardians, _mangeFilesService);
        Classes = new ClassesRepository(_context, _mapper);
        Divisions = new DivisionRepository(_context, _mapper);
        Users = new UsersRepository(_userManager);
        Stages = new StagesRepository(_context, _mapper);
        Accounts = new AccountRepository(_context, _mapper);
        FeeClasses = new FeeClassRepository(_context, _mapper);
        Fees = new FeesRepository(_context, _mapper);
        Tenants = new TenantRepository(_context, _mapper);
        Managers = new ManagerRepository(_context, Users, Tenants, _userManager.PasswordHasher);
        Years = new YearRepository(_context, _mapper);
        Schools = new SchoolRepository(_context, Years, _mapper);
        StudentClassFees = new StudentClassFeeRepository(_context, _mapper);
        Vouchers = new VoucherRepository(_context, _mapper, _mangeFilesService);
        Attachments = new AttachmentsRepository(_context);
        Curriculums = new CurriculumRepository(_context, _mapper);
        CoursePlans = new CoursePlanRepository(_context, _mapper);
        Teachers = new TeacherRepository(_context);
        GradeTypes = new GradeTypesRepository(_context, _mapper);
        Terms = new TermRepository(_context, _mapper);
        Months = new MonthRepository(_context, _mapper);
        Employees = new EmployeeRepository(_context, Users);
        AccountStudentGuardians = new AccountStudentGuardianRepository(_context, _mapper);
        Reports = new ReportRepository(_context);
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

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
