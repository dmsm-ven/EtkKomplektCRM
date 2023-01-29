using System;

namespace EtkBlazorApp.DataAccess.Entity;

public record OrderStatusHistoryEntity(int order_id, DateTime date_time, int old_order_status_id, int new_order_status_id);