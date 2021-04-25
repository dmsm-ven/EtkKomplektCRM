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
        public string FullSizeImage { get; set; }
        public decimal Price { get; set; }
        public decimal BasePrice { get; set; }
        public string BasePriceCurrency { get; set; }
        public string StockStatus { get; set; }   
        public int Quantity { get; set; }
        public string DateModified { get; set; }

        public string ProductIdUri => $"https://etk-komplekt.ru/index.php?route=product/product&product_id={Id}";
    }
}
