using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Services;
using Backend.Services.IServices;


namespace Backend.Extensions
{
    public static class ServiceLayerServiceCollectionExtensions
    {
        public static IServiceCollection AddServicesLayer(this IServiceCollection services)
        {
            services.AddScoped<IClassesServices, ClassesServices>();
            services.AddScoped<IStagesServices, StagesServices>();
            services.AddScoped<IDivisionServices, DivisionServices>();
            services.AddScoped<IFeesServices, FeesServices>();
            services.AddScoped<IFeeClassServices, FeeClassServices>();
            services.AddScoped<IStudentServices, StudentServices>();
            services.AddScoped<IGuardianServices, GuardianServices>();
            services.AddScoped<IUserServices, UsersServices>();
            services.AddScoped<ISchoolServices, SchoolServices>();
            services.AddScoped<IAccountServices, AccountServices>();
            services.AddScoped<IStudentClassFeeServices, StudentClassFeeServices>();
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<IStudentClassFeeServices, StudentClassFeeServices>();
            services.AddScoped<StudentManagementService>();
            services.AddScoped<mangeFilesService>();

            return services;
        }
    }
}