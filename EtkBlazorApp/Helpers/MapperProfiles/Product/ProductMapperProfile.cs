using AutoMapper;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess.Entity;
using System.Web;

namespace EtkBlazorApp.Helpers.MapperProfiles.Product;

public class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {

        CreateMap<ProductViewModel, ProductEntity>()
            .ForMember(p => p.product_id, e => e.MapFrom(p => p.Id))
            .ForMember(p => p.price, e => e.MapFrom(p => p.Price))
            .ForMember(p => p.base_price, e => e.MapFrom(p => p.BasePriceCurrency == CurrencyType.RUB.ToString() ? 0 : p.BasePrice))
            .ForMember(p => p.base_currency_code, e => e.MapFrom(p => p.BasePriceCurrency))
            .ForMember(p => p.quantity, e => e.MapFrom(p => p.Quantity))
            .ForMember(p => p.stock_status, e => e.MapFrom(p => p.StockStatus))
            .ForMember(p => p.replacement_id, e => e.MapFrom(p => p.ReplacementProductId));


        CreateMap<ProductEntity, ProductViewModel>()
            .ForMember(p => p.Id, x => x.MapFrom(p => p.product_id))
            .ForMember(p => p.Image, x => x.MapFrom(p => p.image))
            .ForMember(p => p.Name, x => x.MapFrom(p => HttpUtility.HtmlDecode(p.name) ?? string.Empty))
            .ForMember(p => p.Sku, x => x.MapFrom(p => HttpUtility.HtmlDecode(p.sku) ?? string.Empty))
            .ForMember(p => p.Model, x => x.MapFrom(p => HttpUtility.HtmlDecode(p.model) ?? string.Empty))
            .ForMember(p => p.EAN, x => x.MapFrom(p => p.ean))
            .ForMember(p => p.Manufacturer, x => x.MapFrom(p => p.manufacturer))
            .ForMember(p => p.Price, x => x.MapFrom(p => p.price))
            .ForMember(p => p.BasePrice, x => x.MapFrom(p => p.base_currency_code == CurrencyType.RUB.ToString() ? p.price : p.base_price))
            .ForMember(p => p.BasePriceCurrency, x => x.MapFrom(p => p.base_currency_code))
            .ForMember(p => p.StockStatus, x => x.MapFrom(p => p.stock_status))
            .ForMember(p => p.NumberOfViews, x => x.MapFrom(p => p.viewed))
            .ForMember(p => p.DateModified, x => x.MapFrom(p => p.date_modified))
            .ForMember(p => p.ReplacementProductId, x => x.MapFrom(p => p.replacement_id))
            .ForMember(p => p.Quantity, x => x.MapFrom(p => p.quantity))
            .ForMember(p => p.Uri, x => x.MapFrom(p => !string.IsNullOrWhiteSpace(p.keyword) ? $"https://etk-komplekt.ru/{p.keyword}" : $"https://etk-komplekt.ru/index.php?route=product/product&product_id={p.product_id}"))
            .ForMember(p => p.DiscountedPrice, x => x.MapFrom(p => p.discount_price));
    }
}

