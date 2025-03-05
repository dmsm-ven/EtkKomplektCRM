// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class WBCardListResponse_Card
{
    public int nmID { get; set; }
    public int imtID { get; set; }
    public string nmUUID { get; set; }
    public int subjectID { get; set; }
    public string subjectName { get; set; }
    public string vendorCode { get; set; }
    public string brand { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public List<WBCardListResponse_Photo> photos { get; set; }
    public WBCardListResponse_Dimensions dimensions { get; set; }
    public List<WBCardListResponse_Size> sizes { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class WBCardListResponse_Dimensions
{
    public int width { get; set; }
    public int height { get; set; }
    public int length { get; set; }
}

public class WBCardListResponse_Photo
{
    public string big { get; set; }
    public string c246x328 { get; set; }
    public string c516x688 { get; set; }
    public string square { get; set; }
    public string tm { get; set; }
}

public class WBCardListResponse
{
    public List<WBCardListResponse_Card> cards { get; set; }
    public WBCardListResponse_Cursor cursor { get; set; } = new();
}

public class WBCardListResponse_Cursor
{
    public DateTimeOffset updatedAt { get; set; }
    public int nmID { get; set; } = 0;
    public int total { get; set; } = 0;
}

public class WBCardListResponse_Size
{
    public int chrtID { get; set; }
    public string techSize { get; set; }
    public string wbSize { get; set; }
    public List<string> skus { get; set; }
}

