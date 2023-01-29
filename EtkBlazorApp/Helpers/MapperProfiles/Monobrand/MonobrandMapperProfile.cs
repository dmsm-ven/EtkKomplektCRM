using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;

namespace EtkBlazorApp.MapperProfiles.Monobrand;

public class MonobrandMapperProfile : Profile
{
    public MonobrandMapperProfile()
    {
        CreateMap<MonobrandViewModel, MonobrandEntity>()
            .ForMember(x => x.currency_code, x => x.MapFrom(e => e.CurrencyCode))
            .ForMember(x => x.manufacturer_id, x => x.MapFrom(e => e.ManufacturerId))
            .ForMember(x => x.manufacturer_name, x => x.MapFrom(e => e.ManufacturerName))
            .ForMember(x => x.monobrand_id, x => x.MapFrom(e => e.MonobrandId))
            .ForMember(x => x.website, x => x.MapFrom(e => e.WebsiteUri))
            .ForMember(x => x.is_update_enabled, x => x.MapFrom(e => e.IsUpdateEnabled));

        CreateMap<MonobrandEntity, MonobrandViewModel>()
            .ForMember(x => x.ManufacturerId, x => x.MapFrom(e => e.manufacturer_id))
            .ForMember(x => x.ManufacturerName, x => x.MapFrom(e => e.manufacturer_name ?? "Не выбрано"))
            .ForMember(x => x.MonobrandId, x => x.MapFrom(e => e.monobrand_id))
            .ForMember(x => x.WebsiteUri, x => x.MapFrom(e => e.website))
            .ForMember(x => x.CurrencyCode, x => x.MapFrom(e => e.currency_code))
            .ForMember(x => x.IsUpdateEnabled, x => x.MapFrom(e => e.is_update_enabled));
    }
}
