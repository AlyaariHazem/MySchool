using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOS;
using Backend.DTOS.ClassesDTO;
using Backend.DTOS.DivisionsDTO;
using Backend.DTOS.FeeClassesDTO;
using Backend.DTOS.FeesDTO;
using Backend.DTOS.SchoolsDTO;
using Backend.DTOS.StagesDTO;
using Backend.DTOS.StudentClassFeesDTO;
using Backend.DTOS.StudentsDTO;

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

        CreateMap<UpdateStageDTO, Stage>()
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => DateTime.Now));

        CreateMap<Stage, UpdateStageDTO>().ReverseMap();
        CreateMap<Class, UpdateClassDTO>().ReverseMap();
        CreateMap<Division, UpdateDivisionDTO>().ReverseMap();
        CreateMap<Fee, AddFeeDTO>().ReverseMap();
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



    }
}

