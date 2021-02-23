using AutoMapper;
using CourseLibrary.API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Profiles
{
    public class AuthorsProfiles : Profile
    {

        public AuthorsProfiles()
        {
            CreateMap<Entities.Author, Models.AuthorDto>()
                .ForMember(
                dest => dest.Name,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                 .ForMember(
                dest => dest.Age,
                opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

            CreateMap<Models.AuthorFoCreationDto, Entities.Author>();

        }
    }
}
