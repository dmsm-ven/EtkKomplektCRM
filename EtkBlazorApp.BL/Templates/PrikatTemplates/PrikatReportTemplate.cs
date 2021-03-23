using EtkBlazorApp.Data;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EtkBlazorApp.BL.Managers
{
    public class PrikatReportTemplate
    {
        private string GLN { get; } = "4607804947010";
        private decimal[] DEFAULT_DIMENSIONS = new decimal[] { 150, 100, 100, 0.4m };
        private string LENGTH_UNIT { get; } = "миллиметр";
        private string WEIGHT_UNIT { get; } = "килограмм";

        public string Manufacturer { get; }      
        public CurrencyType Currency { get; }

        public decimal CurrencyRatio { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Discount2 { get; set; }
        readonly int Precission;

        public PrikatReportTemplate(string manufacturer, CurrencyType currency)
        {
            Manufacturer = manufacturer;
            Currency = currency;
            Precission = Currency == CurrencyType.RUB ? 0 : 2;
        }        

        public void AppendLines(List<ProductEntity> products, List<PriceLine> priceLines, bool removeZeroQuantity, StreamWriter writer)
        {
            foreach (var product in products)
            {
                if (removeZeroQuantity && product.quantity <= 0)
                {
                    continue;
                }

                if ((bool)priceLines?.Any())
                {
                    var linkedPriceLine = priceLines?.FirstOrDefault(line => line.Sku.Equals(product.sku, StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrEmpty(line.Model) && (line.Model.Equals(product.model, StringComparison.OrdinalIgnoreCase))) );

                    if (linkedPriceLine != null)
                    {
                        if (!string.IsNullOrWhiteSpace(linkedPriceLine.Name))
                        {
                            product.name = linkedPriceLine.Name;
                        }
                    }
                    else
                    {
                        product.sku = null;
                    }
                }

                if (!string.IsNullOrWhiteSpace(product.sku))
                {
                    AppendLine(product, writer);
                }
            }
        }

        private void AppendLine(ProductEntity product, StreamWriter writer)
        {          
            decimal priceInCurrency = (Currency == CurrencyType.RUB) ? (int)product.price : Math.Round(product.price / CurrencyRatio, Precission);
            decimal price1 = Math.Round(priceInCurrency * ((100m + Discount1) / 100m), Precission);
            decimal price2 = Math.Round((price1 * (100m + Discount2)) / 100m, Precission);
            string recommendedPrice = price1.ToString($"F{Precission}", new CultureInfo("en-EN"));
            string prikatPrice = price2.ToString($"F{Precission}", new CultureInfo("en-EN"));

            writer.Write(GLN + ";");   //GLN поставщика
            writer.Write(";"); //Позиция
            writer.Write(product.ean ?? ";"); //Штрихкод
            writer.Write(";"); //Артикул товара покупателя
            writer.Write(product.sku + ";"); //Артикул товара поставщика
            writer.Write(product.name + ";"); //Наименование
            writer.Write("1" + ";"); //Кол-во
            writer.Write("шт." + ";"); //Единицы измерения
            writer.Write(";"); //Товарная группа
            writer.Write(Manufacturer + ";"); //Бренд
            writer.Write(";"); //Суббренд
            writer.Write(";"); //Вариант названия продукта
            writer.Write(";"); //Функциональное название
            writer.Write(DEFAULT_DIMENSIONS[0].ToString("F2", new CultureInfo("en-EN")) + ";"); //Глубина
            writer.Write(LENGTH_UNIT + ";"); //Единицы измерения
            writer.Write(DEFAULT_DIMENSIONS[1].ToString("F2", new CultureInfo("en-EN")) + ";"); //Ширина
            writer.Write(LENGTH_UNIT + ";"); //Единицы измерения
            writer.Write(DEFAULT_DIMENSIONS[2].ToString("F2", new CultureInfo("en-EN")) + ";"); //Высота
            writer.Write(LENGTH_UNIT + ";"); //Единицы измерения
            writer.Write(";"); //Объем
            writer.Write(";"); //Единицы измерения
            writer.Write(DEFAULT_DIMENSIONS[3].ToString("F4", new CultureInfo("en-EN")) + ";"); //Вес, брутто
            writer.Write(WEIGHT_UNIT + ";"); //Единицы измерения
            writer.Write(";"); //Страна производитель
            writer.Write(";"); //Годен до
            writer.Write(recommendedPrice + ";"); //Рекомендованная цена
            writer.Write(prikatPrice + ";"); //Закупочная цена
            writer.Write(product.quantity.ToString() + ";"); //Количество остатков на скаладе
            writer.Write(Currency.ToString().ToLower() + ";"); //Рекомендованная валюта
            writer.Write(Currency.ToString().ToLower() + ";"); //Закупчная валюта
            writer.WriteLine();
        }
    }
}
