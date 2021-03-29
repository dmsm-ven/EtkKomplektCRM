using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IOrderStorage
    {
        Task<List<OrderEntity>> GetLastOrders(int limit, string number = null, string city = null, string client = null);
        Task<List<OrderDetailsEntity>> GetOrderDetails(int orderId);
        Task<OrderEntity> GetOrderById(int orderId);
    }

    public class OrderStorage : IOrderStorage
    {
        private readonly IDatabaseAccess database;

        public OrderStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<OrderEntity>> GetLastOrders(int takeCount, string order_id = null, string payment_city = null, string shipping_firstname = null)
        {
            if (takeCount <= 0 || takeCount >= 500)
            {
                return new List<OrderEntity>();
            }

            var sb = new StringBuilder()
                .AppendLine("SELECT o.*, s.name as order_status")
                .AppendLine("FROM oc_order o")
                .AppendLine("LEFT JOIN oc_order_status s ON o.order_status_id = s.order_status_id");

            Dictionary<string, string> filter = new Dictionary<string, string>()
            {
                [nameof(order_id)] = order_id,
                [nameof(payment_city)] = payment_city,
                [nameof(shipping_firstname)] = shipping_firstname,
            };

            if (filter.Any(kvp => kvp.Value != null))
            {
                sb.Append($"WHERE ");
                foreach(var kvp in filter.Where(kvp => kvp.Value != null))
                {
                    sb.Append($"o.{kvp.Key} LIKE @{kvp.Key} AND ");
                }
                sb.Remove(sb.Length - 5, 5);
                sb.AppendLine();
            }
            
            sb
                .AppendLine("ORDER BY o.date_added DESC")
                .AppendLine("LIMIT @takeCount");

            string sql = sb.ToString().Trim();

            dynamic parameter = new { takeCount, order_id = $"%{order_id}%", payment_city = $"%{payment_city}%", shipping_firstname = $"%{shipping_firstname}%" };
            var orders = await database.LoadData<OrderEntity, dynamic>(sql, parameter);

            return orders;
        }

        public async Task<List<OrderDetailsEntity>> GetOrderDetails(int orderId)
        {
            string sql = "SELECT op.*, p.sku as sku, m.name as manufacturer FROM oc_order_product op " +
                "LEFT JOIN oc_product p ON op.product_id = p.product_id " +
                "LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id " +
                "WHERE order_id = @Id";

            var details = await database.LoadData<OrderDetailsEntity, dynamic>(sql, new { Id = orderId });

            return details.ToList();
        }

        public async Task<OrderEntity> GetOrderById(int orderId)
        {
            var sql = $"SELECT * FROM oc_order WHERE order_id = @Id";

            var order = (await database.LoadData<OrderEntity, dynamic>(sql, new { Id = orderId })).FirstOrDefault();

            return order;
        }
    }
}
