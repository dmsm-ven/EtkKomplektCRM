using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IOrderStorage
    {
        Task<List<OrderEntity>> GetLastOrders(int limit, string number = null, string city = null, string client = null);
        Task<List<OrderEntity>> GetOrdersFromDate(DateTime startDate);
        Task<List<OrderDetailsEntity>> GetOrderDetailsFromDate(DateTime startDate);
        Task<OrderEntity> GetLastOrder();
        Task<List<OrderDetailsEntity>> GetOrderDetails(int orderId);
        Task<OrderEntity> GetOrderById(int orderId);
        Task<List<OrderEntity>> GetLinkedOrders(int order_id);

        Task<List<OrderStatusEntity>> GetOrderStatuses();

        Task ChangeOrderStatus(int order_id, int old_status_id, int new_status_id);
        Task<List<OrderStatusHistoryEntity>> GetOrderChangeHistory(int order_id);
    }

    public class OrderStorage : IOrderStorage
    {
        private readonly IDatabaseAccess database;

        public OrderStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        //TODO: вынести параметры поиска в отдельный класс фильтра
        public async Task<List<OrderEntity>> GetLastOrders(int takeCount,
            string order_id = null,
            string payment_city = null,
            string shipping_firstname = null)
        {
            if (takeCount <= 0 || takeCount >= 500)
            {
                return new List<OrderEntity>();
            }

            var sb = new StringBuilder()
                .AppendLine("SELECT o.*, otc.cdek_order_number, os.*")
                .AppendLine("FROM oc_order o")
                .AppendLine("LEFT JOIN oc_order_status os ON (o.order_status_id = os.order_status_id AND os.language_id = '2')")
                .AppendLine("LEFT JOIN etk_app_order_to_cdek otc ON (o.order_id = otc.order_id)");

            Dictionary<string, string> filter = new Dictionary<string, string>()
            {
                [nameof(order_id)] = order_id,
                [nameof(payment_city)] = payment_city,
                [nameof(shipping_firstname)] = shipping_firstname,
            };

            if (filter.Any(kvp => kvp.Value != null))
            {
                sb.Append($"WHERE ");
                foreach (var kvp in filter.Where(kvp => kvp.Value != null))
                {
                    sb.Append($"o.{kvp.Key} LIKE @{kvp.Key} AND ");
                }
                sb.Remove(sb.Length - 5, 5);
                sb.AppendLine();
            }

            sb
                .AppendLine("ORDER BY o.order_id DESC")
                .AppendLine("LIMIT @takeCount");

            string sql = sb.ToString().Trim();

            var orders = await database.GetListWithChild<OrderEntity, OrderStatusEntity, dynamic>(sql,
                splitColumnName: "order_status_id",
                new
                {
                    takeCount,
                    order_id = $"%{order_id}%",
                    payment_city = $"%{payment_city}%",
                    shipping_firstname = $"%{shipping_firstname}%"
                });

            return orders;
        }

        public async Task<List<OrderEntity>> GetOrdersFromDate(DateTime startDate)
        {
            string sql = @"SELECT o.*, s.name as order_status
                           FROM oc_order o
                           LEFT JOIN oc_order_status s ON o.order_status_id = s.order_status_id
                           WHERE date_added >= @startDate";
            var orders = await database.GetList<OrderEntity, dynamic>(sql, new { startDate });
            return orders;
        }

        public async Task<List<OrderDetailsEntity>> GetOrderDetailsFromDate(DateTime startDate)
        {
            string sql = @"SELECT op.*, p.sku as sku, m.name as manufacturer 
                          FROM oc_order o
                          LEFT JOIN oc_order_product op ON o.order_id = op.order_id 
                          LEFT JOIN oc_product p ON op.product_id = p.product_id
                          LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                          WHERE o.date_added >= @startDate";

            var details = await database.GetList<OrderDetailsEntity, dynamic>(sql, new { startDate });

            return details;
        }

        public async Task<OrderEntity> GetOrderById(int orderId)
        {
            var sql = new StringBuilder()
                .AppendLine("SELECT o.*, osf.inn, otc.cdek_order_number, os.*")
                .AppendLine("FROM oc_order o")
                .AppendLine("LEFT JOIN oc_order_status os ON (o.order_status_id = os.order_status_id)")
                .AppendLine("LEFT JOIN oc_order_simple_fields osf ON (o.order_id = osf.order_id)")
                .AppendLine("LEFT JOIN etk_app_order_to_cdek otc ON (o.order_id = otc.order_id)")
                .AppendLine("WHERE o.order_id = @orderId")
                .ToString();

            var order = await database.GetSingleWithChild<OrderEntity, OrderStatusEntity, dynamic>(sql, "order_status_id", new { orderId });

            order.details = await GetOrderDetails(orderId);

            return order;
        }

        public async Task<List<OrderDetailsEntity>> GetOrderDetails(int orderId)
        {
            string sql = @"SELECT op.*, p.sku as sku, m.name as manufacturer 
                           FROM oc_order_product op
                           LEFT JOIN oc_product p ON op.product_id = p.product_id
                           LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id
                           WHERE order_id = @orderId";

            var details = await database.GetList<OrderDetailsEntity, dynamic>(sql, new { orderId });

            return details;
        }

        public async Task<List<OrderEntity>> GetLinkedOrders(int order_id)
        {
            var order = await GetOrderById(order_id);

            var sql = @"SELECT * FROM oc_order
                      WHERE (email = @email OR telephone = @telephone OR ip = @ip)
                      ORDER BY date_added DESC";

            var linkedOrders = await database.GetList<OrderEntity, OrderEntity>(sql, order);

            return linkedOrders;
        }

        public async Task<OrderEntity> GetLastOrder()
        {
            var sql = "SELECT * FROM oc_order ORDER BY order_id DESC LIMIT 1";
            var order = await database.GetFirstOrDefault<OrderEntity>(sql);
            return order;
        }

        public async Task<List<OrderStatusEntity>> GetOrderStatuses()
        {
            var statuses = await database.GetList<OrderStatusEntity>("SELECT * FROM oc_order_status ORDER BY order_status_sort");

            return statuses;
        }

        public async Task ChangeOrderStatus(int order_id, int old_status_id, int new_status_id)
        {
            string historySql = new StringBuilder()
                .AppendLine("INSERT INTO etk_app_order_status_change_history (order_id, old_order_status_id, new_order_status_id)")
                .AppendLine("VALUES (@order_id, @old_status_id, @new_status_id)")
                .ToString();

            await database.ExecuteQuery(historySql, new { order_id, old_status_id, new_status_id });

            string statusSql = "UPDATE oc_order SET order_status_id = @new_status_id WHERE order_id = @order_id";
            await database.ExecuteQuery(statusSql, new { order_id, new_status_id });
        }

        public async Task<List<OrderStatusHistoryEntity>> GetOrderChangeHistory(int order_id)
        {
            string sql = "SELECT * FROM etk_app_order_status_change_history WHERE order_id = @order_id";

            var historyItems = await database.GetList<OrderStatusHistoryEntity, dynamic>(sql, new { order_id });

            return historyItems;
        }
    }
}
