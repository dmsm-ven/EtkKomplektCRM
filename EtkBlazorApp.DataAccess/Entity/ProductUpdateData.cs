using System.Collections.Generic;

namespace EtkBlazorApp.DataAccess
{
    public class ProductUpdateData
    {
        public int product_id { get; set; }      
        public decimal? price { get; set; }       
        public string currency_code { get; set; }

        public int? quantity { get; set; }
        public int stock_id { get; set; }

        //Только для типа 'MultistockPriceLine'
        public Dictionary<int, int> AdditionalStocksQuantity { get; set; } = null;

        //Только для типа 'PriceLine WithNextDeliveryDate' - Ожидается количество через X через Y дней
        public NextStockDelivery NextStockDelivery { get; set; } = null;
    }
}
