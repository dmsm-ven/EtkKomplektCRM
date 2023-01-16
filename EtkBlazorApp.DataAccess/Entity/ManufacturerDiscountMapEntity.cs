namespace EtkBlazorApp.DataAccess.Entity
{
    public class ManufacturerDiscountMapEntity
    {
        public string price_list_guid { get; set; }
        public decimal discount { get; set; }
        public int manufacturer_id { get; set; }
        public string name { get; set; }
    }
}
