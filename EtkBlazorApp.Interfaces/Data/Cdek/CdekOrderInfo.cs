namespace EtkBlazorApp.Core.Data.Cdek;

public class CdekOrderInfoRoot
{
    public CdekOrderInfo entity { get; set; }
}

public class CdekOrderInfo
{
    public string cdek_number { get; set; }
    public string comment { get; set; }
    public string delivery_point { get; set; }

    public CdekOrderInfo_Recipient recipient { get; set; }
    public CdekOrderInfo_ToLocation to_location { get; set; }
    public CdekOrderInfo_DeliveryDetail delivery_detail { get; set; }
}

public class CdekOrderInfo_Recipient
{
    public string company { get; set; }
    public string name { get; set; }
    public CdekOrderInfo_Recipient_Phone[] phones { get; set; }
}

public class CdekOrderInfo_Recipient_Phone
{
    public string number { get; set; }
}
public class CdekOrderInfo_ToLocation
{
    public string city { get; set; }
    public string country { get; set; }
    public string region { get; set; }
    public string address { get; set; }
}
public class CdekOrderInfo_DeliveryDetail
{
    public decimal delivery_sum { get; set; }
    public decimal total_sum { get; set; }
}
