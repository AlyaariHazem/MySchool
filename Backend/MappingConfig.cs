using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOS;
using Backend.DTOS.School;
using Backend.DTOS.School.Accounts;
using Backend.DTOS.School.AccountStudentGuardian;
using Backend.DTOS.School.Classes;
using Backend.DTOS.School.CoursePlan;
using Backend.DTOS.School.Curriculms;
using Backend.DTOS.School.FeeClass;
using Backend.DTOS.School.Fees;
using Backend.DTOS.School.GradeTypes;
using Backend.DTOS.School.Guardians;
using Backend.DTOS.School.MonthlyGrade;
using Backend.DTOS.School.Months;
using Backend.DTOS.School.Stages;
using Backend.DTOS.School.StudentClassFee;
using Backend.DTOS.School.Students;
using Backend.DTOS.School.Subjects;
using Backend.DTOS.School.Tenant;
using Backend.DTOS.School.TermlyGrade;
using Backend.DTOS.School.Terms;
using Backend.DTOS.School.Vouchers;
using Backend.DTOS.School.Years;
using Backend.Models;

namespace Backend;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        CreateMap<Class, ClassDTO>()
            .ForMember(dest => dest.Divisions, opt => opt.MapFrom(src => src.Divisions)) // Map Divisions to DivisionINClassDTO
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.Divisions.Sum(c => c.Students.Count))) // Calculate total student count per class
            .ForMember(dest => dest.StageName, opt => opt.MapFrom(src => src.Stage.StageName));

        CreateMap<Stage, StageDTO>()
            .ForMember(dest => dest.Classes, opt => opt.MapFrom(src => src.Classes)) // Mapping to ClassDTO
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.Classes.Sum(c => c.FeeClasses != null ? c.FeeClasses
            .Sum(fc => fc.StudentClassFees != null ? fc.StudentClassFees.Count : 0) : 0)));


        CreateMap<Class, AddClassDTO>().ReverseMap();

        CreateMap<Class, ClassInStageDTO>()
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.FeeClasses.Sum(s => s.StudentClassFees.Count))); // Counting students in each class

        CreateMap<Division, DivisionINClassDTO>();

        CreateMap<StagesDTO, Stage>()
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => DateTime.Now));

        CreateMap<Stage, StagesDTO>().ReverseMap();
        CreateMap<Class, UpdateClassDTO>().ReverseMap();
        CreateMap<Division, UpdateDivisionDTO>().ReverseMap();
        CreateMap<Fee, GetFeeDTO>().ReverseMap();
        CreateMap<Student, StudentDetailsDTO>().ReverseMap();
        CreateMap<School, SchoolDTO>().ReverseMap();
        CreateMap<StudentClassFeeDTO, StudentClassFees>().ReverseMap();
        CreateMap<Fee, FeeDTO>().ReverseMap();
        CreateMap<FeeClass, AddFeeClassDTO>().ReverseMap();
        CreateMap<FeeClass, FeeClassDTO>()
        .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.ClassName))
        .ForMember(dest => dest.FeeNameAlis, opt => opt.MapFrom(src => src.Fee.FeeNameAlis))
        .ForMember(dest => dest.FeeName, opt => opt.MapFrom(src => src.Fee.FeeName))
        .ForMember(dest => dest.ClassYear, opt => opt.MapFrom(src => src.Class.ClassYear));
        CreateMap<FeeClass, AddFeeClassDTO>().ReverseMap();
        CreateMap<Year, YearDTO>().ReverseMap();
        CreateMap<Tenant, TenantDTO>().ReverseMap();
        CreateMap<Guardian, GuardianDTO>().ReverseMap();
        CreateMap<Accounts, AccountsDTO>().ReverseMap();
        CreateMap<Subject, SubjectsDTO>().ReverseMap();
        CreateMap<VouchersDTO, Vouchers>()
       .ForMember(dest => dest.Attachments, opt => opt.Ignore());

        CreateMap<Vouchers, VouchersDTO>()
            .ForMember(dest => dest.Attachments, opt => opt
                .MapFrom(src => src.Attachments.Select(a => a.AttachmentURL).ToList()));

        CreateMap<Subject, SubjectsDTO>().ReverseMap();
        CreateMap<Curriculum, CurriculumDTO>().ReverseMap();
        CreateMap<CoursePlan, CoursePlanDTO>().ReverseMap();

        // CurriculumReturnDTO mapping from Curriculum entity
            CreateMap<Curriculum, CurriculumReturnDTO>()
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.SubjectName))
                .ForMember(dest => dest.CurriculumName, opt => opt.MapFrom(src => src.CurriculumName))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.ClassName))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
                .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate));

        //  Mapping for DTOs    
        CreateMap<GradeTypesDTO, GradeType>().ReverseMap();
        CreateMap<MonthlyGradeDTO, MonthlyGrade>().ReverseMap();
        CreateMap<TermlyGrade, TermlyGradeDTO>().ReverseMap();
        CreateMap<Term, TermDTO>().ReverseMap();
        CreateMap<MonthDTO, Month>().ReverseMap();
        CreateMap<SubjectsNameDTO, Subject>().ReverseMap();
        CreateMap<AllClassesDTO, Class>().ReverseMap();
        CreateMap<AccountStudentGuardian, AccountStudentGuardianDTO>().ReverseMap();
        CreateMap<FeeClass, ChangeStateFeeClassDTO>().ReverseMap();
        CreateMap<Fee, ChangeStateFeeDTO>().ReverseMap();

    }
}

