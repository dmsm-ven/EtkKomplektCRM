using System;
using System.Collections.Generic;

namespace EtkBlazorApp.Data
{
    public class OrderViewModel
    {
        public DateTime DateTime { get; set; }
        public string OrderId { get; set; }
        public string City { get; set; }
        public string Customer { get; set; }
        public decimal TotalPrice { get; set; }      
        public string OrderStatus { get; set; }

        public bool IsDone => Equals(OrderStatus, "Завершен");

        public List<OrderDetailsViewModel> OrderDetails { get; set; } 
    }

    public class OrderDetailsViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Model { get; set; }
        public string Sku { get; set; }
        public string Manufacturer { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal Sum => Quantity * Price;
    }
}
