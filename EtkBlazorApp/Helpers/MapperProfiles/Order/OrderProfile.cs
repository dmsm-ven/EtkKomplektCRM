using AutoMapper;
using EtkBlazorApp.Core.Data.Order;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Model.Order;
using System.Web;

namespace EtkBlazorApp.MapperProfiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderStatusHistoryEntity, OrderStatusHistoryViewModel>()
            .ForMember(o => o.OrderHistoryId, o => o.MapFrom(x => x.order_id))
            .ForMember(o => o.DateAdded, o => o.MapFrom(x => x.date_added))
            .ForMember(o => o.OrderId, o => o.MapFrom(x => x.order_id))
            .ForMember(o => o.Comment, o => o.MapFrom(x => x.comment))
            .ForMember(o => o.OrderStatusId, o => o.MapFrom(x => x.order_status_id))
            .ForMember(o => o.Notify, o => o.MapFrom(x => x.notify))
            .ForMember(o => o.StatusName, o => o.MapFrom(x => x.status_name));

        CreateMap<OrderStatusEntity, OrderStatusViewModel>()
            .ForMember(o => o.Type, o => o.MapFrom(x => (OrderStatusCode)x.order_status_id))
            .ForMember(o => o.Name, o => o.MapFrom(x => x.name))
            .ForMember(o => o.SortOrder, o => o.MapFrom(x => x.order_status_sort));

        CreateMap<OrderEntity, OrderViewModel>()
            .ForMember(o => o.OrderId, o => o.MapFrom(x => x.order_id.ToString()))
            .ForMember(o => o.City, o => o.MapFrom(x => x.payment_city))
            .ForMember(o => o.Customer, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.payment_firstname)))
            .ForMember(o => o.DateTime, o => o.MapFrom(x => x.date_added))
            .ForMember(o => o.TotalPrice, o => o.MapFrom(x => x.total))
            .ForMember(o => o.Comment, o => o.MapFrom(x => x.comment))
            .ForMember(o => o.CustomerEmail, o => o.MapFrom(x => x.email))
            .ForMember(o => o.CustomerPhoneNumber, o => o.MapFrom(x => x.telephone))
            .ForMember(o => o.ShippingAddress, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.shipping_address_1)))
            .ForMember(o => o.PaymentMethod, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.payment_method)))
            .ForMember(o => o.ShippingMethod, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.shipping_method)))
            .ForMember(o => o.Inn, o => o.MapFrom(x => x.inn ?? string.Empty))
            .ForMember(o => o.CdekOrderNumber, o => o.MapFrom(x => x.cdek_order_number))
            .ForMember(o => o.Status, o => o.MapFrom(x => x.order_status))
            .ForMember(o => o.OrderDetails, o => o.MapFrom(x => x.details))
            .ForMember(o => o.StatusChangesHistory, o => o.MapFrom(x => x.status_changes_history));
    }
}
