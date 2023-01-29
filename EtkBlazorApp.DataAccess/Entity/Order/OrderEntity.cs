using System;
using System.Collections.Generic;
using System.Text;

namespace EtkBlazorApp.DataAccess.Entity;

public class OrderEntity
{
    public DateTime date_added { get; set; }
    public int order_id { get; set; }
    public int order_status_id { get; set; }
    public string payment_city { get; set; }
    public string payment_firstname { get; set; }
    public decimal total { get; set; }

    public string firstname { get; set; }
    public string lastname { get; set; }
    public string email { get; set; }
    public string telephone { get; set; }
    public string payment_zone { get; set; }
    public string payment_method { get; set; }
    public string shipping_address_1 { get; set; }
    public string shipping_method { get; set; }
    public string comment { get; set; }
    public string ip { get; set; }

    public List<OrderDetailsEntity> details { get; set; } = new();
    public List<OrderTagEntity> tags { get; set; } = new();
    public List<OrderStatusHistoryEntity> status_changes_history { get; set; } = new();
    public OrderStatusEntity order_status { get; set; }
}
