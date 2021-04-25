using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp
{
    public class OrderViewModel
    {
        public DateTime DateTime { get; set; }
        public string OrderId { get; set; }
        public string City { get; set; }
        public string Customer { get; set; }
        public decimal TotalPrice { get; set; }      
        public string OrderStatus { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string Comment { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }

        public bool IsDone => Equals(OrderStatus, "Завершен");
        public decimal ShipmentCost => TotalPrice - OrderDetails.Sum(od => od.Sum);
        public int ProductsTotalQuantity => OrderDetails.Sum(od => od.Quantity);
        public decimal ProductsTotalCost => OrderDetails.Sum(od => od.Sum);

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
