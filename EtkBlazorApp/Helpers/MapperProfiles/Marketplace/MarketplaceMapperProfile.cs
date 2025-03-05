using AutoMapper;
using EtkBlazorApp.DataAccess.Entity.Manufacturer;
using EtkBlazorApp.Model.Marketplace;

namespace EtkBlazorApp.Helpers.MapperProfiles.Marketplace;

public class MarketplaceMapperProfile : Profile
{
    public MarketplaceMapperProfile()
    {
        CreateMap<MarketplaceStepDiscountEntity, MarketplaceStepDiscountModel>()
            .ForMember(entity => entity.MinPriceInRub, model => model.MapFrom(x => x.price_border_in_rub))
            .ForMember(entity => entity.Ratio, model => model.MapFrom(x => x.ratio))
            .ForMember(entity => entity.Marketplace, model => model.MapFrom(x => x.marketplace))
            .ReverseMap();
    }
}

