using System.ComponentModel;

namespace EtkBlazorApp
{
    public enum ChartDateRange
    { 
    
        [Description("За неделю")] Week,
        [Description("За месяц")] Month,
        [Description("За год")] Year
    }

    public enum ChartKind
    {
        [Description("По производителям")] ByManufacturer,
        [Description("По клиентам")] ByCustomer,
        [Description("По товарам")] ByProduct,
        [Description("По городам")] ByCity
    }
}
