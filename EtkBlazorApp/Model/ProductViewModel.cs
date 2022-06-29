using System;
using System.ComponentModel.DataAnnotations;

namespace EtkBlazorApp
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Sku { get; set; }
        public string EAN { get; set; }
        public string Uri { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public decimal BasePrice { get; set; }
        public string BasePriceCurrency { get; set; }
        public string StockStatus { get; set; }   
        public int NumberOfViews { get; set; }
        public int Quantity { get; set; }
        public string DateModified { get; set; }

        public int? ReplacementProductId { get; set; }
        public string ReplacementProductName { get; set; }

        public string FullSizeImage => $"https://etk-komplekt.ru/image/{Image}";
        public double? DiscountPercent
        {
            get
            {
                if(DiscountedPrice.HasValue && Price != decimal.Zero)
                {
                    return (1d - (double)Math.Round(DiscountedPrice.Value / Price, 2));
                }
                return null;
            }
        }
        public string ProductIdUri => $"https://etk-komplekt.ru/index.php?route=product/product&product_id={Id}";
    }
}
