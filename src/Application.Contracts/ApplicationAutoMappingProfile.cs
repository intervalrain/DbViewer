using Application.Contracts.Dtos;
using AutoMapper;
using Domain.Database.Entities;
using Domain.Security;

namespace Application.Contracts;

public class ApplicationAutoMappingProfile : Profile
{
    public ApplicationAutoMappingProfile()
    {
        CreateMap<ConnectionInfo, ConnectionInfoDto>();
        CreateMap<ConnectionInfoDto, ConnectionInfo>();
        
        CreateMap<UserInfo, UserInfoDto>();
        CreateMap<UserInfoDto, UserInfo>();
    }
}