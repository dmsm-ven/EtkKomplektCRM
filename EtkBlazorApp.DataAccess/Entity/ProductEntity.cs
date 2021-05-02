using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.DataAccess.Entity
{
    public class ProductEntity
    {
        public int product_id { get; set; }
        public string name { get; set; }
        public string model { get; set; }
        public string manufacturer { get; set; }
        public string sku { get; set; }
        public string ean { get; set; }
        public string keyword { get; set; }
        public decimal price { get; set; }
        public decimal? discount_price { get; set; }
        public decimal base_price { get; set; }
        public string base_currency_code { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        public string stock_status { get; set; }
        public int quantity { get; set; }
        public int viewed { get; set; }

        public decimal length { get; set; }
        public decimal width { get; set; }
        public decimal height { get; set; }
        public decimal weight { get; set; }

        public DateTime? date_modified { get; set; }
    }
}
