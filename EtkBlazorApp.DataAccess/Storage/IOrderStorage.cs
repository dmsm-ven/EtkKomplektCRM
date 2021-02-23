using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IOrderStorage
    {
        Task<List<OrderEntity>> GetLastOrders(int limit, string city = null);
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

        public async Task<List<OrderEntity>> GetLastOrders(int takeCount, string city = null)
        {
            if (takeCount <= 0 || takeCount >= 500)
            {
                return new List<OrderEntity>();
            }

            var sb = new StringBuilder()
                .AppendLine("SELECT o.*, s.name as order_status")
                .AppendLine("FROM oc_order o")
                .AppendLine("LEFT JOIN oc_order_status s ON o.order_status_id = s.order_status_id");

            if (!string.IsNullOrWhiteSpace(city))
            {
                sb.AppendLine("WHERE o.payment_city = @City");
            }
            sb
                .AppendLine("ORDER BY o.date_added DESC")
                .AppendLine("LIMIT @TakeCount");

            string sql = sb.ToString().Trim();
            var orders = await database.LoadData<OrderEntity, dynamic>(sql, new { TakeCount = takeCount, City = city });
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
