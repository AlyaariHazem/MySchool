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
    private readonly IGuardianRepository _guardianRepository;
    private readonly mangeFilesService _mangeFilesService;
    private readonly ITenantRepository _tenantRepository;
    private readonly IYearRepository _yearRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public UnitOfWork(
        DatabaseContext context,
        IMapper mapper,
        IGuardianRepository guardianRepository,
        mangeFilesService mangeFilesService,
        ITenantRepository tenantRepository,
        IYearRepository yearRepository,
        UserManager<ApplicationUser> userManager
    )
    {
        _context = context;
        _mapper = mapper;
        _guardianRepository = guardianRepository;
        _mangeFilesService = mangeFilesService;
        _tenantRepository = tenantRepository;
        _yearRepository = yearRepository;
        _userManager = userManager;

        Subjects = new SubjectRepository(_context, _mapper);
        Students = new StudentRepository(_context, _guardianRepository, _mangeFilesService);
        Classes = new ClassesRepository(_context, _mapper);
        Divisions = new DivisionRepository(_context, _mapper);
        Guardians = new GuardianRepository(_context, _mapper);
        Users = new UsersRepository(_userManager);
        Stages = new StagesRepository(_context, _mapper);
        Accounts = new AccountRepository(_context, _mapper);
        FeeClasses = new FeeClassRepostory(_context, _mapper);
        Fees = new FeesRepository(_context, _mapper);
        Managers = new ManagerRepository(_context, Users, _tenantRepository, _userManager.PasswordHasher);
        Schools = new SchoolRepository(_context, _yearRepository, _mapper);
        StudentClassFees = new StudentClassFeeRepository(_context, _mapper);
        Tenants = new TenantRepository(_context, _mapper);
        Years = new YearRepository(_context, _mapper);
    }

    public ISubjectRepository Subjects { get; private set; }
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

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
