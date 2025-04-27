using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Repository;
using Backend.Repository.School;
using Backend.Repository.School.Implements;
using Backend.Repository.School.Interfaces;

namespace Backend.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ISubjectsRepository Subjects { get; }
    IStudentRepository Students { get; }
    IClassesRepository Classes { get; }
    IDivisionRepository Divisions { get; }
    IGuardianRepository Guardians { get; }
    IUserRepository Users { get; }
    IStagesRepository Stages { get; }
    IAccountRepository Accounts { get; }
    IFeeClassRepository FeeClasses { get; }
    IFeesRepository Fees { get; }
    IManagerRepository Managers { get; }
    ISchoolRepository Schools { get; }
    IStudentClassFeeRepository StudentClassFees { get; }
    ITenantRepository Tenants { get; }
    IYearRepository Years { get; }
    IVoucherRepository Vouchers { get; }
    IAttachmentRepository Attachments { get; }
    ICurriculumRepository Curriculums { get; }
    ICoursePlanRepository CoursePlans { get; }
    ITeacherRepository Teachers { get; }
    IGradeTypesRepository GradeTypes { get; }
    ITermRepository Terms { get; }
    IMonthRepository Months { get; }
    IEmployeeRepository Employees { get; }
    IAccountStudentGuardianRepository AccountStudentGuardians { get; }
    Task<int> CompleteAsync();
}
