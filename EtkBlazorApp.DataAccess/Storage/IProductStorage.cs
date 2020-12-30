using EtkBlazorApp.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface IProductStorage
    {
        Task<List<ProductEntity>> GetLastAddedProducts(int count);
        ProductEntity GetProductById(int id);
    }

    public class ProductStorage : IProductStorage
    {
        private readonly IDatabaseAccess database;
        private readonly int MAX_LIMIT_PER_REQUEST = 100;

        public ProductStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<ProductEntity>> GetLastAddedProducts(int count)
        {
            if (count > MAX_LIMIT_PER_REQUEST)
            {
                throw new ArgumentOutOfRangeException($"Превышено максимальное количество запрашиваемых товаров ({MAX_LIMIT_PER_REQUEST})");
            }

            var sb = new StringBuilder()
                .AppendLine("SELECT p.*, d.name as name, m.name as manufacturer")
                .AppendLine("FROM oc_product p")
                .AppendLine("LEFT JOIN oc_product_description d ON p.product_id = d.product_id")
                .AppendLine("LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                .AppendLine("ORDER BY Date(p.date_added) DESC")
                .AppendLine("LIMIT @Limit");

            string sql = sb.ToString();

            var products = await database.LoadData<ProductEntity, dynamic>(sql, new { Limit = count });

            return products.ToList();
        }

        public ProductEntity GetProductById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
