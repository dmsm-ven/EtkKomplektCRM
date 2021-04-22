using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EtkBlazorApp.BL.Templates
{
    public abstract class PrikatReportTemplateBase
    {
        public static string PRIKAT_ONLY_PREFIX  => "PRIKAT_ONLY-";

        public bool IsProductInStock { get; set; }
        public bool IsProductHasEan { get; set; }
        public decimal CurrencyRatio { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Discount2 { get; set; }

        protected virtual string GLN { get; } = "4607804947010";
        protected virtual decimal[] DEFAULT_DIMENSIONS { get; } = new decimal[] { 150, 100, 100, 0.4m }; 
        protected virtual string LENGTH_UNIT { get; } = "миллиметр";
        protected virtual string WEIGHT_UNIT { get; } = "килограмм";

        protected int Precission { get; }
        public string Manufacturer { get; }
        public CurrencyType Currency { get; }

        public PrikatReportTemplateBase(string manufacturer, CurrencyType currency)
        {
            Manufacturer = manufacturer;
            Currency = currency;
            Precission = (Currency == CurrencyType.RUB ? 0 : 2);
        }        

        public void AppendLines(List<ProductEntity> products, List<PriceLine> priceLines, StreamWriter writer)
        {
            foreach (var product in products)
            {
                if (IsProductInStock && product.quantity <= 0)
                {
                    continue;
                }

                if(IsProductHasEan && string.IsNullOrWhiteSpace(product.ean))
                {
                    continue;
                }

                if (priceLines.Any())
                {
                    var linkedPriceLine = priceLines?
                        .FirstOrDefault(line => line.Sku.Equals(product.sku, StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrEmpty(line.Model) && (line.Model.Equals(product.model, StringComparison.OrdinalIgnoreCase))) );

                    if (linkedPriceLine != null)
                    {
                        if (!string.IsNullOrWhiteSpace(linkedPriceLine.Name))
                        {
                            product.name = linkedPriceLine.Name.Replace(";", " ").Trim();
                        }
                        product.price = linkedPriceLine.Price.Value * CurrencyRatio;
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

        protected virtual void AppendLine(ProductEntity product, StreamWriter sw)
        {          
            decimal priceInCurrency = (Currency == CurrencyType.RUB) ? (int)product.price : Math.Round(product.price / CurrencyRatio, Precission);
            decimal price1 = Math.Round(priceInCurrency * ((100m + Discount1) / 100m), Precission);
            decimal price2 = Math.Round((price1 * (100m + Discount2)) / 100m, Precission);
            
            string recommendedPrice = price1.ToString($"F{Precission}", new CultureInfo("en-EN"));
            string prikatPrice = price2.ToString($"F{Precission}", new CultureInfo("en-EN"));

            string length = (product.length != decimal.Zero ? product.length : DEFAULT_DIMENSIONS[0]).ToString("F2", new CultureInfo("en-EN"));
            string width = (product.width != decimal.Zero ? product.width : DEFAULT_DIMENSIONS[1]).ToString("F2", new CultureInfo("en-EN"));
            string height = (product.height != decimal.Zero ? product.height : DEFAULT_DIMENSIONS[2]).ToString("F2", new CultureInfo("en-EN"));
            string weight = (product.weight != decimal.Zero ? product.weight : DEFAULT_DIMENSIONS[3]).ToString("F4", new CultureInfo("en-EN"));

            WriteCell(sw, GLN);   //GLN поставщика
            WriteCell(sw); //Позиция
            WriteCell(sw, product.ean); //Штрихкод
            WriteCell(sw); //Артикул товара покупателя
            WriteCell(sw, product.sku); //Артикул товара поставщика
            WriteCell(sw, product.name); //Наименование
            WriteCell(sw, "1"); //Кол-во
            WriteCell(sw, "шт."); //Единицы измерения
            WriteCell(sw); //Товарная группа
            WriteCell(sw, Manufacturer); //Бренд
            WriteCell(sw); //Суббренд
            WriteCell(sw); //Вариант названия продукта
            WriteCell(sw); //Функциональное название
            WriteCell(sw, length); //Глубина
            WriteCell(sw, LENGTH_UNIT); //Единицы измерения
            WriteCell(sw, width); //Ширина
            WriteCell(sw, LENGTH_UNIT); //Единицы измерения
            WriteCell(sw, height); //Высота
            WriteCell(sw, LENGTH_UNIT); //Единицы измерения
            WriteCell(sw); //Объем
            WriteCell(sw); //Единицы измерения
            WriteCell(sw, weight); //Вес, брутто
            WriteCell(sw, WEIGHT_UNIT); //Единицы измерения
            WriteCell(sw); //Страна производитель
            WriteCell(sw); //Годен до
            WriteCell(sw, recommendedPrice); //Рекомендованная цена
            WriteCell(sw, prikatPrice); //Закупочная цена
            WriteCell(sw, product.quantity.ToString()); //Количество остатков на скаладе
            WriteCell(sw, Currency.ToString().ToLower()); //Рекомендованная валюта
            WriteCell(sw, Currency.ToString().ToLower()); //Закупчная валюта
            sw.WriteLine();

        }

        private void WriteCell(StreamWriter sw, string value = null)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sw.Write(value.Replace(";", " ")?.Trim());
            }
            sw.Write(";");
        }
    }
}
