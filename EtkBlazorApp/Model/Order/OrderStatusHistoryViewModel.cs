using System;

namespace EtkBlazorApp.Model.Order
{
    public class OrderStatusHistoryViewModel
    {
        public int OrderHistoryId { get; set; }
        public int OrderId { get; set; }
        public int OrderStatusId { get; set; }
        public bool Notify { get; set; }
        public string Comment { get; set; }
        public string StatusName { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
