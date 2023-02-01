using System;

namespace EtkBlazorApp.DataAccess.Entity.PriceList;

public class PriceListUpdateEntryEntity
{
    public int update_id { get; set; }
    public Guid price_list_id { get; set; }
    public DateTime date_time { get; set; }
}
