using EtkBlazorApp.DataAccess.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.DataAccess
{
    public interface ICategoryStorage
    {
        Task<List<CategoryEntity>> SearchCategoriesByName(string searchText);
    }

    public class CategoryStorage : ICategoryStorage
    {
        private readonly IDatabaseAccess database;

        public CategoryStorage(IDatabaseAccess database)
        {
            this.database = database;
        }

        public async Task<List<CategoryEntity>> SearchCategoriesByName(string searchText)
        {
            var sql = @"SELECT c.category_id, cd.name
                        FROM oc_category c
                        JOIN oc_category_description cd ON c.category_id = cd.category_id
                        WHERE c.status = 1 AND cd.name LIKE @pattern
                        ORDER BY cd.name
                        LIMIT 10";

            var findedProducts = await database.GetList<CategoryEntity, dynamic>(sql, new { pattern = $"%{searchText}%" });

            return findedProducts;
        }
    }
}
