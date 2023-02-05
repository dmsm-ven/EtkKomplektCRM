using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;

namespace EtkBlazorApp.Helpers.MapperProfiles;

public class AppUserProfile : Profile
{
    public AppUserProfile()
    {
        CreateMap<AppUser, AppUserEntity>()
            .ForMember(u => u.user_id, x => x.MapFrom(u => u.Id))
            .ForMember(u => u.login, x => x.MapFrom(u => u.Login))
            .ForMember(u => u.status, x => x.MapFrom(u => u.IsEnabled))
            .ForMember(u => u.user_group_id, x => x.MapFrom(u => u.GroupId))
            .ForMember(u => u.password, x => x.MapFrom(u => u.Password))
            .ForMember(u => u.ip, x => x.MapFrom(u => string.IsNullOrWhiteSpace(u.AllowedIp) ? null : u.AllowedIp));

        CreateMap<AppUserEntity, AppUser>()
            .ForMember(u => u.Id, x => x.MapFrom(u => u.user_id))
            .ForMember(u => u.CreatingDate, x => x.MapFrom(u => u.creation_date))
            .ForMember(u => u.Login, x => x.MapFrom(u => u.login))
            .ForMember(u => u.LastLoginDateTime, x => x.MapFrom(u => u.last_login_date))
            .ForMember(u => u.GroupName, x => x.MapFrom(u => u.group_name))
            .ForMember(u => u.GroupId, x => x.MapFrom(u => u.user_group_id))
            .ForMember(u => u.IsEnabled, x => x.MapFrom(u => u.status))
            .ForMember(u => u.AllowedIp, x => x.MapFrom(u => u.ip));
    }
}

