using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Backend.DTOS;
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
            .ForMember(dest => dest.StageName, opt => opt.MapFrom(src => src.Stage.StageName)); // How can I Select the StageName?

        // Mapping Stage -> StageDTO with ClassDTO for Classes property
        CreateMap<Stage, StageDTO>()
            .ForMember(dest => dest.Classes, opt => opt.MapFrom(src => src.Classes)) // Mapping to ClassDTO
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.Classes.Sum(c => c.StudentClass.Count))); // Calculate total student count per stage
        
        CreateMap<Class, AddClassDTO>().ReverseMap();
        
        // Map Class -> ClassInStageDTO
        CreateMap<Class, ClassInStageDTO>()
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.StudentClass.Count)); // Counting students in each class

        // Map Division -> DivisionINClassDTO
        CreateMap<Division, DivisionINClassDTO>();

        CreateMap<StagesDTO, Stage>()
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => DateTime.Now));
            
        CreateMap<Stage,StagesDTO>().ReverseMap();
        CreateMap<Class,UpdateClassDTO>().ReverseMap();
    }
}

