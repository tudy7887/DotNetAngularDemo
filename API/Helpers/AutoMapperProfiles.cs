using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
        .ForMember(user => user.Age,
                   option => option.MapFrom(source => source.DateOfBirth.CalculateAge()))
        .ForMember(user => user.PhotoUrl,
                   option => option.MapFrom(source => source.Photos.FirstOrDefault(photo => photo.IsMain)!.Url));
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<Message, MessageDto>()
        .ForMember(message => message.SenderPhotoUrl, 
                  option => option.MapFrom(source => source.Sender.Photos.FirstOrDefault(photo => photo.IsMain)!.Url))
        .ForMember(message => message.RecipientPhotoUrl, 
                  option => option.MapFrom(source => source.Recipient.Photos.FirstOrDefault(photo => photo.IsMain)!.Url));
        CreateMap<MessageDto, Message>();
    }
}
