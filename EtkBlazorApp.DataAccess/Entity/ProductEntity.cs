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
        public decimal price { get; set; }
        public string url { get; set; }
        public string image { get; set; }
        public int quantity { get; set; }
    }
}
