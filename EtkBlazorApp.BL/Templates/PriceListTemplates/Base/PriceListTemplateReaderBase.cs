using EtkBlazorApp.DataAccess.Entity.PriceList;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PriceListTemplates.Base
{
    public abstract class PriceListTemplateReaderBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        //TODO: возможно стоит убрать все словари и оставить только Пост-проверки (при считывании всего прайс-листа в целом)
        protected IReadOnlyDictionary<string, string> ManufacturerNameMap { get; private set; }
        protected IReadOnlyDictionary<string, int> QuantityMap { get; private set; }
        protected IReadOnlyDictionary<string, decimal> ManufacturerDiscountMap { get; private set; }
        protected IReadOnlyList<string> BrandsWhiteList { get; private set; }
        protected IReadOnlyList<string> BrandsBlackList { get; private set; }

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
                if (decimal.TryParse(strValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    price = Math.Max(parsedPrice, 0);
                    if (roundDigits.HasValue)
                    {
                        price = Math.Round(price.Value, roundDigits.Value);
                    }
                }
            }

            return canBeNull ? price : price ?? 0;
        }

        protected virtual int? ParseQuantity(string str, bool canBeNull = false)
        {
            int? quantity = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (decimal.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsedQuantity))
                {
                    quantity = Math.Max((int)parsedQuantity, 0);
                }
                else if (QuantityMap.ContainsKey(str))
                {
                    quantity = QuantityMap[str];
                }
            }

            return canBeNull ? quantity : quantity ?? 0;
        }

        /// <summary>
        /// Проверка на белый/черный список производителя. Данный функионал так же дублируется на последнем этапе загрузки прайс-листа. Так что, даже если не добавлять проверку внутри цикла создания PriceLine, производиль который был добавлен в черный/белый список все равно в итоге НЕ будет загружен. Но раняя проверка позволяет не делать кучу ненужных вычислений в огромных прайс-листах и сразу отбросить ненужный бренд и перейти к следующей строке (не загружать их в память) 
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        protected bool SkipThisBrand(string manufacturer)
        {
            bool blackListCondition = BrandsBlackList.Any() && BrandsBlackList.Contains(manufacturer, StringComparer.OrdinalIgnoreCase);
            bool whiteListCondition = BrandsWhiteList.Any() && BrandsWhiteList.Contains(manufacturer, StringComparer.OrdinalIgnoreCase) == false;

            return blackListCondition || whiteListCondition;
        }

        /// <summary>
        /// Заполняем словари для шаблона прайс-листа, для того что бы можно было, прямо во время парсинга строки, в прайс-листе, ее изменить/отбросить без загрузки в память
        /// </summary>
        /// <param name="templateInfo"></param>
        public void FillTemplateInfo(PriceListTemplateEntity templateInfo)
        {
            Dictionary<string, string> manufacturerNameMap = new(StringComparer.OrdinalIgnoreCase);
            if (templateInfo.manufacturer_name_map != null)
            {
                foreach (var kvp in templateInfo.manufacturer_name_map.Where(i => i != null))
                {
                    if (!string.IsNullOrWhiteSpace(kvp.text) && !string.IsNullOrWhiteSpace(kvp.name) && !manufacturerNameMap.ContainsKey(kvp.text))
                    {
                        manufacturerNameMap[kvp.text] = kvp.name;
                    }
                }
            }
            ManufacturerNameMap = manufacturerNameMap;

            Dictionary<string, int> quantityMap = new(StringComparer.OrdinalIgnoreCase);
            if (templateInfo.quantity_map != null)
            {
                foreach (var kvp in templateInfo.quantity_map.Where(i => i != null))
                {
                    if (!string.IsNullOrWhiteSpace(kvp.text) && !quantityMap.ContainsKey(kvp.text))
                    {
                        quantityMap[kvp.text] = kvp.quantity;
                    }
                }
            }
            QuantityMap = quantityMap;

            Dictionary<string, decimal> manufacturerDiscountMap = new(StringComparer.OrdinalIgnoreCase);
            if (templateInfo.manufacturer_discount_map != null)
            {
                foreach (var kvp in templateInfo.manufacturer_discount_map.Where(i => i != null))
                {
                    if (!string.IsNullOrWhiteSpace(kvp.name) && !manufacturerDiscountMap.ContainsKey(kvp.name))
                    {
                        manufacturerDiscountMap[kvp.name] = kvp.discount;
                    }
                }
            }
            ManufacturerDiscountMap = manufacturerDiscountMap;

            HashSet<string> brandsWhiteList = new();
            HashSet<string> brandsBlackList = new();
            if (templateInfo.manufacturer_skip_list != null && templateInfo.manufacturer_skip_list.Any())
            {
                var listsSource = templateInfo.manufacturer_skip_list
                    .Where(i => !string.IsNullOrWhiteSpace(i.name) && !string.IsNullOrWhiteSpace(i.list_type))
                    .Where(i => new[] { "black_list", "white_list" }.Contains(i.list_type));

                foreach (var item in listsSource)
                {
                    if (item.list_type == "black_list")
                    {
                        brandsBlackList.Add(item.name);
                    }
                    if (item.list_type == "white_list")
                    {
                        brandsWhiteList.Add(item.name);
                    }
                }
            }
            BrandsWhiteList = brandsWhiteList.ToList();
            BrandsBlackList = brandsBlackList.ToList();
        }
    }
}
