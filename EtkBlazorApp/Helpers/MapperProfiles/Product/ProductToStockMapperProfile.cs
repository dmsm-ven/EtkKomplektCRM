using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;

namespace EtkBlazorApp.Helpers.MapperProfiles.Product;

public class ProductToStockMapperProfile : Profile
{
    public ProductToStockMapperProfile()
    {
        CreateMap<ProductToStockDataModel, ProductToStockEntity>()
            .ForMember(x => x.product_id, x => x.MapFrom(s => s.ProductId))
            .ForMember(x => x.stock_partner_id, x => x.MapFrom(s => s.StockId))
            .ForMember(x => x.price, x => x.MapFrom(s => s.Price))
            .ForMember(x => x.original_price, x => x.MapFrom(s => s.OriginalPrice))
            .ForMember(x => x.quantity, x => x.MapFrom(s => s.Quantity))
            .ForMember(x => x.currency_code, x => x.MapFrom(s => s.PriceCurrency));

        CreateMap<ProductToStockEntity, ProductToStockDataModel>()
            .ForMember(x => x.ProductId, x => x.MapFrom(s => s.product_id))
            .ForMember(x => x.StockId, x => x.MapFrom(s => s.stock_partner_id))
            .ForMember(x => x.Price, x => x.MapFrom(s => s.price))
            .ForMember(x => x.OriginalPrice, x => x.MapFrom(s => s.original_price))
            .ForMember(x => x.Quantity, x => x.MapFrom(s => s.quantity))
            .ForMember(x => x.StockName, x => x.MapFrom(s => s.stock_name))
            .ForMember(x => x.PriceCurrency, x => x.MapFrom(s => s.currency_code));
    }
}

