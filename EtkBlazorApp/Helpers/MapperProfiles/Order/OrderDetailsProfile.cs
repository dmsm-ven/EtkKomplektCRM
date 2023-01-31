using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;
using System.Web;

namespace EtkBlazorApp.Helpers.MapperProfiles.Order;

public class OrderDetailsProfile : Profile
{
    public OrderDetailsProfile()
    {
        CreateMap<OrderDetailsEntity, OrderDetailsViewModel>()
            .ForMember(o => o.ProductName, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.name)))
            .ForMember(o => o.Model, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.model)))
            .ForMember(o => o.Sku, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.sku)))
            .ForMember(o => o.Price, o => o.MapFrom(x => x.price))
            .ForMember(o => o.ProductId, o => o.MapFrom(x => x.product_id))
            .ForMember(o => o.Quantity, o => o.MapFrom(x => x.quantity))
            .ForMember(o => o.Manufacturer, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.manufacturer)));
    }
}

