using System;
using System.Collections.Generic;
using System.Linq;

namespace EtkBlazorApp;

public class Order
{
    public DateTime DateTime { get; set; }
    public int OrderId { get; set; }
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
    public string Inn { get; set; }

    public string TkOrderNumber { get; set; }
    public string TkCode { get; set; }



    //TODO: проверить, возможно есть какие-то другие скрытые суммы при расчете
    public decimal ShipmentCost => TotalPrice - OrderDetails.Sum(od => od.Sum);
    public int ProductsTotalQuantity => OrderDetails.Sum(od => od.Quantity);
    public decimal ProductsTotalCost => OrderDetails.Sum(od => od.Sum);

    public OrderStatus Status { get; set; }
    public List<OrderDetails> OrderDetails { get; set; } = new();
    public List<OrderStatusHistoryEntry> StatusChangesHistory { get; set; } = new();
}

