using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
  public class AutoMapperProfiles : Profile
  {
    public AutoMapperProfiles()
    {
      // use AutoMapper to bind user entity with dto

      // configure photoUrl in the process of mapping
      // get the source(AppUser)'s frist photo and check it's main photo
      // also, get the source age and calcylate age then map it as well
      CreateMap<AppUser, MemberDto>()
        .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
          src.Photos.FirstOrDefault(x => x.IsMain).Url))
        .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
          src.DateOfBirth.CaculateAge()));
      CreateMap<Photo, PhotoDto>();
    }

  }
}