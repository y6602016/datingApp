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

      // CreateMap<a, b> map from a to b


      // configure photoUrl in the process of mapping
      // get the source(AppUser)'s frist photo and check it's main photo
      // also, get the source age and calcylate age then map it as well
      CreateMap<AppUser, MemberDto>()
        .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
          src.Photos.FirstOrDefault(x => x.IsMain).Url))
        .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
          src.DateOfBirth.CaculateAge()));
      CreateMap<Photo, PhotoDto>();
      CreateMap<MemberUpdateDto, AppUser>();
      CreateMap<RegisterDto, AppUser>();
      CreateMap<Message, MessageDto>()
        .ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src =>
          src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
        .ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src =>
          src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
      // when we return time to the client, we'll append 'z' at the end of the time to indicate it's Utc time
      // CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
    }

  }
}