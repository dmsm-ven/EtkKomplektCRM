using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;
using System.Web;

namespace EtkBlazorApp.Model.MapperProfiles.OrderProfiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderEntity, OrderViewModel>()
            .ForMember(o => o.OrderId, o => o.MapFrom(x => x.order_id.ToString()))
            .ForMember(o => o.City, o => o.MapFrom(x => x.payment_city))
            .ForMember(o => o.Customer, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.payment_firstname)))
            .ForMember(o => o.DateTime, o => o.MapFrom(x => x.date_added))
            .ForMember(o => o.TotalPrice, o => o.MapFrom(x => x.total))
            .ForMember(o => o.OrderStatusName, o => o.MapFrom(x => x.order_status))
            .ForMember(o => o.OrderStatusType, o => o.MapFrom(x => x.order_status_id));
    }
}
