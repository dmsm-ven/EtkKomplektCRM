using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess.Entity;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates.Base
{
    public abstract class PrikatReportTemplateBase
    {
        private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

        public decimal CurrencyRatio { get; set; }
        public decimal Discount1 { get; set; }
        public decimal Discount2 { get; set; }
        public string GLN { get; set; }

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
            Precission = Currency == CurrencyType.RUB ? 0 : 2;
        }

        public void WriteProductLines(IEnumerable<ProductEntity> products, StreamWriter sw)
        {
            foreach (var product in products)
            {
                WriteProductLine(product, sw);
            }
        }

        protected virtual void WriteProductLine(ProductEntity product, StreamWriter sw)
        {
            decimal priceInCurrency = (int)product.price;
            if (Currency != CurrencyType.RUB)
            {
                priceInCurrency = product.base_currency_code == Currency.ToString() && product.base_price != decimal.Zero ?
                    product.base_price :
                    Math.Round(product.price / CurrencyRatio, 2);
            }

            decimal price1 = Math.Round(priceInCurrency * ((100m + Discount2) / 100m), Precission);
            decimal price2 = Math.Round(price1 * (100m + Discount1) / 100m, Precission);

            string vi_price_rrc = price1.ToString($"F{Precission}", new CultureInfo("en-EN"));
            string vi_price = price2.ToString($"F{Precission}", new CultureInfo("en-EN"));

            string length = (product.length != decimal.Zero ? product.length : DEFAULT_DIMENSIONS[0]).ToString("F2", new CultureInfo("en-EN"));
            string width = (product.width != decimal.Zero ? product.width : DEFAULT_DIMENSIONS[1]).ToString("F2", new CultureInfo("en-EN"));
            string height = (product.height != decimal.Zero ? product.height : DEFAULT_DIMENSIONS[2]).ToString("F2", new CultureInfo("en-EN"));
            string weight = (product.weight != decimal.Zero ? product.weight : DEFAULT_DIMENSIONS[3]).ToString("F4", new CultureInfo("en-EN"));

            //TODO: ИЗМЕНИТЬ, изначально сделано неправильно - тут должен быть артикул поставщика (НАШ SKU), 
            //А не артикул поставщика у того которого мы покупаем товар
            //Правильный вариант, у всех товаров должен быть вида: ETK-123456
            string sku =
                !string.IsNullOrWhiteSpace(product.sku) ? product.sku :
                (!string.IsNullOrWhiteSpace(product.model) ? product.model :
                $"ETK-{product.product_id}");

            WriteCell(sw, GLN);   //GLN поставщика
            WriteCell(sw); //Позиция
            WriteCell(sw, product.ean); //Штрихкод
            WriteCell(sw); //Артикул товара покупателя
            WriteCell(sw, sku); //Артикул товара поставщика 
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
            WriteCell(sw, vi_price_rrc); //Рекомендованная цена
            WriteCell(sw, vi_price); //Закупочная цена
            WriteCell(sw, product.quantity.ToString()); //Количество остатков на скаладе
            WriteCell(sw, Currency.ToString().ToLower()); //Рекомендованная валюта
            WriteCell(sw, Currency.ToString().ToLower()); //Закупчная валюта
            sw.WriteLine();
        }

        protected void WriteCell(StreamWriter sw, string value = null)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sw.Write(value.Replace(";", " ")?.Trim());
            }
            sw.Write(";");
        }
    }
}
