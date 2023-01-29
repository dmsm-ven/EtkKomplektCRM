using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;
using System.Web;

namespace EtkBlazorApp.Helpers.MapperProfiles.Product;

public class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {
        CreateMap<ProductEntity, ProductViewModel>()
            .ForMember(p => p.Image, x => x.MapFrom(p => p.image))
            .ForMember(p => p.Name, x => x.MapFrom(p => HttpUtility.HtmlDecode(p.name) ?? string.Empty))
            .ForMember(p => p.Manufacturer, x => x.MapFrom(p => p.manufacturer))
            .ForMember(p => p.Price, x => x.MapFrom(p => p.price))
            .ForMember(p => p.Id, x => x.MapFrom(p => p.product_id))
            .ForMember(p => p.NumberOfViews, x => x.MapFrom(p => p.viewed))
            .ForMember(p => p.DiscountedPrice, x => x.MapFrom(p => p.discount_price));
    }
}

