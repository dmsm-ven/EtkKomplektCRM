namespace EtkBlazorApp.DataAccess.Entity
{
    public class CategoryEntity
    {
        public int category_id { get; set; }
        public int parent_id { get; set; }
        public string name { get; set; }
    }
}
