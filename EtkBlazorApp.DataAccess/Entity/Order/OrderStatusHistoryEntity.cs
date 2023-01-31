using System;

namespace EtkBlazorApp.DataAccess.Entity;

public class OrderStatusHistoryEntity
{
    public int order_history_id { get; set; }
    public int order_id { get; set; }
    public int order_status_id { get; set; }
    public bool notify { get; set; }
    public string comment { get; set; }
    public string status_name { get; set; }
    public DateTime date_added { get; set; }
};