using EtkBlazorApp.Model.Order;
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
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string Comment { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderStatusName { get; set; }


        //TODO: проверить, возможно есть какие-то другие скрытые суммы при расчете
        public decimal ShipmentCost => TotalPrice - OrderDetails.Sum(od => od.Sum);
        public int ProductsTotalQuantity => OrderDetails.Sum(od => od.Quantity);
        public decimal ProductsTotalCost => OrderDetails.Sum(od => od.Sum);

        public OrderStatusViewModel Status { get; set; }
        public List<OrderDetailsViewModel> OrderDetails { get; set; }
        public List<OrderTagViewModel> Tags { get; set; }
    }
}
