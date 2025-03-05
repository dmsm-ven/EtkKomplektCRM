// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class WBQuantityUpdatePayload
{
    public List<WBQuantityUpdatePayload_Stock> stocks { get; set; }
}

public class WBQuantityUpdatePayload_Stock
{
    public string sku { get; set; }
    public int amount { get; set; }
}

public class WBQuantityClearPayload
{
    public List<string> skus { get; set; } = new();
}