using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Repository;
using Backend.Repository.IRepository;

namespace Backend.Extensions
{
    public static class RepositoryLayerServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositoriesLayer(this IServiceCollection services)
        {
            // Register custom repositories
            services.AddScoped<IClassesRepository, ClassesRepository>();
            services.AddScoped<IStagesRepository, StagesRepository>();
            services.AddScoped<IDivisionRepository, DivisionRepository>();
            services.AddScoped<IFeesRepository, FeesRepository>();
            services.AddScoped<IFeeClassRepository, FeeClassRepostory>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IGuardianRepository, GuardianRepository>();
            services.AddScoped<IUserRepository, UsersRepository>();
            services.AddScoped<ISchoolRepository, SchoolRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IStudentClassFeeRepository, StudentClassFeeRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IStudentClassFeeRepository, StudentClassFeeRepository>();

            return services;
        }
    }
}