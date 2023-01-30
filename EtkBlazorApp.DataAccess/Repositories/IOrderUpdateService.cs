using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IOrderUpdateService
    {
        Task ChangeOrderStatus(int order_id, int old_status_id, int new_status_id);
        Task ChangeOrderLinkedCdekOrderNumber(int order_id, string cdek_order_number);
    }

    public class OrderUpdateService : IOrderUpdateService
    {
        private readonly IDatabaseAccess database;

        public OrderUpdateService(IDatabaseAccess database)
        {
            this.database = database;
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

        public async Task ChangeOrderLinkedCdekOrderNumber(int order_id, string cdek_order_number)
        {
            string removeOldSql = "DELETE FROM etk_app_order_to_cdek WHERE order_id = @order_id";
            await database.ExecuteQuery(removeOldSql, new { order_id });

            string addOrderNumberSql = "INSERT INTO etk_app_order_to_cdek (order_id, cdek_order_number) VALUES (@order_id, @cdek_order_number)";
            await database.ExecuteQuery(addOrderNumberSql, new { order_id, cdek_order_number });
        }
    }
}
