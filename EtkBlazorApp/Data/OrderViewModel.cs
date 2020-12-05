using System;

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
    }
}
