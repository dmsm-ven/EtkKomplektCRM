using System.Text.Json.Serialization;

public class WBCardList_Filter
{
    public int withPhoto { get; set; } = -1;
}

public class WBCardListPayload
{
    public WBCardList_Settings settings { get; set; } = new();
}
public class WBCardListPayloadFactory
{
    public static WBCardListPayload Empty => new();

    public static WBCardListPayload GetPayloadWithPagination(int limit, DateTimeOffset updatedAt, int nmID)
    {
        var payload = new WBCardListPayload();
        payload.settings.cursor = new WBCardList_CursorWithPage()
        {
            limit = limit,
            updatedAt = updatedAt,
            nmID = nmID
        };
        return payload;
    }
}

public class WBCardList_Settings
{
    public WBCardList_Filter filter { get; set; } = new();
    public WBCardList_CursorBase cursor { get; set; } = new();
}

public class WBCardList_CursorWithPage : WBCardList_CursorBase
{
    public DateTimeOffset updatedAt { get; set; }
    public int nmID { get; set; } = 0;
}

[JsonDerivedType(typeof(WBCardList_CursorWithPage))]
public class WBCardList_CursorBase
{
    public int limit { get; set; } = 100;
}