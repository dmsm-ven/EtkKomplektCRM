public class WBCardList_Cursor
{
    public int limit { get; set; } = 100;
}

public class WBCardList_Filter
{
    public int withPhoto { get; set; } = -1;
}

public class WBCardListPayload
{
    public WBCardList_Settings settings { get; set; } = new WBCardList_Settings();

    public static WBCardListPayload Empty => new();
}

public class WBCardList_Settings
{
    public WBCardList_Filter filter { get; set; } = new WBCardList_Filter();
    public WBCardList_Cursor cursor { get; set; } = new WBCardList_Cursor();
}

