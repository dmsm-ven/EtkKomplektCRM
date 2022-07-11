using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EtkBlazorApp.BL
{
    public abstract class PriceListTemplateReaderBase
    {
        //TODO: Тут стоит поменят словари и списки на ReadOnly версии
        protected Dictionary<string, string> ManufacturerNameMap { get; private set; }
        protected Dictionary<string, int> QuantityMap { get; private set; }
        protected List<string> BrandsWhiteList { get; private set; }
        protected List<string> BrandsBlackList { get; private set; }

        public PriceListTemplateReaderBase()
        {
            ManufacturerNameMap = new Dictionary<string, string>();
            QuantityMap = new Dictionary<string, int>();
            BrandsWhiteList = new List<string>();
            BrandsBlackList = new List<string>();
        }

        protected string MapManufacturerName(string manufacturerName)
        {
            if (!string.IsNullOrWhiteSpace(manufacturerName) && ManufacturerNameMap.Any() && ManufacturerNameMap.ContainsKey(manufacturerName))
            {
                return ManufacturerNameMap[manufacturerName];
            }
            return manufacturerName;
        }

        protected virtual decimal? ParsePrice(string str, bool canBeNull = false, int? roundDigits = null)
        {
            decimal? price = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                string strValue = str.Replace(",", ".").Replace(" ", string.Empty);
                if (decimal.TryParse(strValue, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    price = Math.Max(parsedPrice, 0);
                    if (roundDigits.HasValue)
                    {
                        price = Math.Round(price.Value, roundDigits.Value);
                    }
                }
            }

            return canBeNull ? price : (price ?? 0);
        }

        protected virtual int? ParseQuantity(string str, bool canBeNull = false)
        {
            int? quantity = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedQuantity))
                {
                    quantity = Math.Max((int)parsedQuantity, 0);
                }
                else if(QuantityMap.ContainsKey(str))
                {
                    quantity = QuantityMap[str];
                }
            }

            return canBeNull ? quantity : (quantity ?? 0);
        }

        /// <summary>
        /// Проверка на пропуск из загрузки прайс-листа. Скорее всего нужно перенести эту проверки на момент после загрузки прайс-листа, там можно будет убрать проверку из каждого шаблона и оставить только в одном месте
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        protected bool ManufacturerSkipCheck(string manufacturer)
        {
            bool blackListCondition = BrandsBlackList.Any() && BrandsBlackList.Contains(manufacturer, StringComparer.OrdinalIgnoreCase);
            bool whiteListCondition = BrandsWhiteList.Any() && (BrandsWhiteList.Contains(manufacturer, StringComparer.OrdinalIgnoreCase) == false);

            return blackListCondition || whiteListCondition;
        }

        public void FillTemplateInfo(PriceListTemplateEntity templateInfo)
        {
            if (templateInfo.quantity_map != null)
            {
                foreach (var kvp in templateInfo.quantity_map)
                {
                    QuantityMap[kvp.text] = kvp.quantity;
                }
            }
            if (templateInfo.manufacturer_name_map != null)
            {
                foreach (var kvp in templateInfo.manufacturer_name_map)
                {
                    ManufacturerNameMap[kvp.text] = kvp.name;
                }
            }
            if (templateInfo.manufacturer_skip_list != null)
            {
                var listsSource = templateInfo.manufacturer_skip_list.Where(i => !string.IsNullOrWhiteSpace(i.name) && !string.IsNullOrWhiteSpace(i.list_type));
                foreach (var item in listsSource)
                {
                    if (item.list_type == "black_list" && !BrandsBlackList.Contains(item.name))
                    {
                        BrandsBlackList.Add(item.name);
                    }
                    if (item.list_type == "white_list" && !BrandsWhiteList.Contains(item.name))
                    {
                        BrandsWhiteList.Add(item.name);
                    }
                }
            }
        }
    }
}
