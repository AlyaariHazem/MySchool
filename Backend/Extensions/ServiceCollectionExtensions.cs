using Backend.Interfaces;
using Backend.Repository;
using Backend.Repository.School;
using Backend.Repository.School.Classes;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;
using Backend.Services;
using FirstProjectWithMVC.Repository.School;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Services
        services.AddScoped<StudentManagementService>();
        services.AddScoped<mangeFilesService>();
        services.AddScoped<StudentClassFeesRepository>();
        services.AddScoped<TenantProvisioningService>();
        services.AddScoped<HtmlSanitizationService>();

        // Register Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGuardianRepository, GuardianRepository>();
        services.AddScoped<ISubjectsRepository, SubjectRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IClassesRepository, ClassesRepository>();
        services.AddScoped<IDivisionRepository, DivisionRepository>();
        services.AddScoped<IUserRepository, UsersRepository>();
        services.AddScoped<IStagesRepository, StagesRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IFeeClassRepository, FeeClassRepository>();
        services.AddScoped<IFeesRepository, FeesRepository>();
        services.AddScoped<IManagerRepository, ManagerRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IStudentClassFeeRepository, StudentClassFeeRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IYearRepository, YearRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentsRepository>();
        services.AddScoped<ICurriculumRepository, CurriculumRepository>();
        services.AddScoped<ICoursePlanRepository, CoursePlanRepository>();
        services.AddScoped<IGradeTypesRepository, GradeTypesRepository>();
        services.AddScoped<IMonthlyGradeRepository, MonthlyGradeRepository>();
        services.AddScoped<ITermlyGradeRepository, TermlyGradeRepository>();
        services.AddScoped<ITermRepository, TermRepository>();
        services.AddScoped<IMonthRepository, MonthRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IAccountStudentGuardianRepository, AccountStudentGuardianRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        return services;
    }
}
