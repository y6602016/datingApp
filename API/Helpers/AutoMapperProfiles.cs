using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers
{
  public class AutoMapperProfiles : Profile
  {
    public AutoMapperProfiles()
    {
      // use AutoMapper to bind user entity with dto
      CreateMap<AppUser, MemberDto>();
      CreateMap<Photo, PhotoDto>();
    }

  }
}