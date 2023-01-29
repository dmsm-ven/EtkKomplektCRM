using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Model.Order;
using System.Web;

namespace EtkBlazorApp.MapperProfiles;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<OrderStatusEntity, OrderStatusViewModel>()
            .ForMember(o => o.Type, o => o.MapFrom(x => (OrderStatusType)x.order_status_id))
            .ForMember(o => o.Name, o => o.MapFrom(x => x.name))
            .ForMember(o => o.SortOrder, o => o.MapFrom(x => x.order_status_sort));

        CreateMap<OrderTagEntity, OrderTagViewModel>()
            .ForMember(o => o.Type, o => o.MapFrom(x => (OrderTagType)x.order_tag_id))
            .ForMember(o => o.Name, o => o.MapFrom(x => x.name))
            .ForMember(o => o.Description, o => o.MapFrom(x => x.description));

        CreateMap<OrderDetailsEntity, OrderDetailsViewModel>()
            .ForMember(o => o.ProductName, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.name)))
            .ForMember(o => o.Model, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.model)))
            .ForMember(o => o.Sku, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.sku)))
            .ForMember(o => o.Price, o => o.MapFrom(x => x.price))
            .ForMember(o => o.ProductId, o => o.MapFrom(x => x.product_id))
            .ForMember(o => o.Quantity, o => o.MapFrom(x => x.quantity))
            .ForMember(o => o.Manufacturer, o => o.MapFrom(x => HttpUtility.HtmlDecode(x.manufacturer)));

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
            .ForMember(o => o.Status, o => o.MapFrom(x => x.order_status))
            .ForMember(o => o.OrderDetails, o => o.MapFrom(x => x.details))
            .ForMember(o => o.Tags, o => o.MapFrom(x => x.tags));
    }
}
