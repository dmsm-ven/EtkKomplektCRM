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
        Task UpdateStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate);
        Task UpdatePrice(List<ProductUpdateData> data);
        Task<List<ProductEntity>> ReadProducts();
    }

    public class ProductStorage : IProductStorage
    {
        private readonly IDatabaseAccess database;
        
        public ProductStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<ProductEntity>> GetLastAddedProducts(int count)
        {
            int MAX_LIMIT_PER_REQUEST = 100;

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

        public async Task<List<ProductEntity>> ReadProducts()
        {
            //public int product_id { get; set; }
            //public string name { get; set; }
            //public string model { get; set; }
            //public string manufacturer { get; set; }
            //public string sku { get; set; }
            //public decimal price { get; set; }
            //public string url { get; set; }
            //public string image { get; set; }
            //public int quantity { get; set; }
            //public DateTime date_modified { get; set; }
            //public DateTime date_added { get; set; }

            var sb = new StringBuilder()
                    .Append("SELECT p.*, d.name as name, m.name as manufacturer, url.keyword as url")
                    .Append("FROM oc_product p")
                    .Append("LEFT JOIN oc_product_description d ON p.product_id = d.product_id")
                    .Append("LEFT JOIN oc_manufacturer m ON p.manufacturer_id = m.manufacturer_id")
                    .Append("LEFT JOIN oc_url_alias url ON CONCAT('product_id=', p.product_id) = [url.query]");

            string sql = sb.ToString();

            var products = await database.LoadData<ProductEntity, dynamic>(sql, new { });

            return products;

        }

        public async Task UpdatePrice(List<ProductUpdateData> data)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateStock(List<ProductUpdateData> data, bool clearStockBeforeUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
