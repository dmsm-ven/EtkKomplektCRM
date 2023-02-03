namespace EtkBlazorApp.Core.Data.Cdek;



public class CdekWebhookOrderStatusData
{
    public string type { get; set; }
    public string date_time { get; set; }
    public string uuid { get; set; }
    public CdekWebhookOrderStatusData_Attributes attributes { get; set; }
}

public class CdekWebhookOrderStatusData_Attributes
{
    public bool is_return { get; set; }
    public string cdek_number { get; set; }
    public string code { get; set; }
    public string status_code { get; set; }
    public string status_date_time { get; set; }
    public string city_name { get; set; }
    public string city_code { get; set; }
}
