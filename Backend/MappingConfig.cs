using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOS;
using Backend.DTOS.School.Fees;
using Backend.DTOS.School.Stages;
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

        // Mapping Stage -> StageDTO with ClassDTO for Classes property
        CreateMap<Stage, StageDTO>()
            .ForMember(dest => dest.Classes, opt => opt.MapFrom(src => src.Classes)) // Mapping to ClassDTO
            .ForMember(dest => dest.StudentCount,opt => opt.MapFrom(src => src.Classes.Sum(c => c.FeeClasses != null? c.FeeClasses
            .Sum(fc => fc.StudentClassFees != null? fc.StudentClassFees.Count: 0): 0)));

        
        CreateMap<Class, AddClassDTO>().ReverseMap();
        
        // Map Class -> ClassInStageDTO
        CreateMap<Class, ClassInStageDTO>()
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.FeeClasses.Sum(s=>s.StudentClassFees.Count))); // Counting students in each class

        // Map Division -> DivisionINClassDTO
        CreateMap<Division, DivisionINClassDTO>();

        CreateMap<StagesDTO, Stage>()
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => DateTime.Now));
            
        CreateMap<Stage,StagesDTO>().ReverseMap();
        CreateMap<Class,UpdateClassDTO>().ReverseMap();
        CreateMap<Division,UpdateDivisionDTO>().ReverseMap();
        CreateMap<Fee,GetFeeDTO>().ReverseMap();
        
        CreateMap<Fee,FeeDTO>().ReverseMap();
        CreateMap<FeeClass,AddFeeClassDTO>().ReverseMap();
        CreateMap<FeeClass, FeeClassDTO>()
        .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.Class.ClassName)) 
        .ForMember(dest => dest.FeeNameAlis, opt => opt.MapFrom(src => src.Fee.FeeNameAlis)) 
        .ForMember(dest => dest.FeeName, opt => opt.MapFrom(src => src.Fee.FeeName)) 
        .ForMember(dest => dest.ClassYear, opt => opt.MapFrom(src => src.Class.ClassYear));
        CreateMap<FeeClass, AddFeeClassDTO>().ReverseMap()
        .ForMember(dest => dest.FeeID, opt => opt.MapFrom(src => src.FeeID))
        .ForMember(dest => dest.ClassID, opt => opt.MapFrom(src => src.ClassID));

        
    }
}

