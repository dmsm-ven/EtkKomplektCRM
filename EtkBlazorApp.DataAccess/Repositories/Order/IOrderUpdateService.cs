using EtkBlazorApp.Core.Data;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IOrderUpdateService
    {
        /// <summary>
        /// Меняет статус заказа и заносит изменение о новом татусе
        /// </summary>
        /// <param name="order_id"></param>
        /// <param name="order_status_id"></param>
        /// <returns></returns>
        Task ChangeOrderStatus(int order_id, int order_status_id);
        Task ChangeOrderLinkedTkNumber(int order_id, string order_number, TransportDeliveryCompany tk);
    }

    public class OrderUpdateService : IOrderUpdateService
    {
        private readonly IDatabaseAccess database;

        public OrderUpdateService(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task ChangeOrderStatus(int order_id, int order_status_id)
        {
            string historySql = new StringBuilder()
                .AppendLine("INSERT INTO oc_order_history (order_id, order_status_id, comment, date_added)")
                .AppendLine("VALUES (@order_id, @order_status_id, @comment, @date_added)")
                .ToString();

            await database.ExecuteQuery(historySql, new { order_id, order_status_id, comment = "LK", date_added = DateTime.Now });

            string statusSql = "UPDATE oc_order SET order_status_id = @order_status_id WHERE order_id = @order_id";
            await database.ExecuteQuery(statusSql, new { order_id, order_status_id });
        }

        public async Task ChangeOrderLinkedTkNumber(int order_id, string order_number, TransportDeliveryCompany tk)
        {
            string removeOldSql = "DELETE FROM etk_app_order_to_tk_order WHERE order_id = @order_id";
            await database.ExecuteQuery(removeOldSql, new { order_id });

            string addOrderNumberSql = $"INSERT INTO etk_app_order_to_tk_order (order_id, tk_order_number, tk_code) VALUES (@order_id, @order_number, @tk_code)";
            await database.ExecuteQuery(addOrderNumberSql, new { order_id, order_number, tk_code = tk.ToString().ToLower() });
        }
    }
}
