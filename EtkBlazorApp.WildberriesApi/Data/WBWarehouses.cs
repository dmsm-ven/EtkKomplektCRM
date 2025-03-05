namespace EtkBlazorApp.WildberriesApi.Data;
public class WBWarehouseList : List<WBWarehouse>
{

}
public class WBWarehouse
{
    public string name { get; set; }
    public int officeId { get; set; }
    public int id { get; set; }
    public int cargoType { get; set; }
    public int deliveryType { get; set; }
}
