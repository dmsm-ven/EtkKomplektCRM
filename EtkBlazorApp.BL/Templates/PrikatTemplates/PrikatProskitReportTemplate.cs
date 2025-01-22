using EtkBlazorApp.BL.Templates.PrikatTemplates.Base;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EtkBlazorApp.BL.Templates.PrikatTemplates
{
    //В этом шаблоне, в отличии от обычных, не выгружаем РРЦ цена (всегда должна быть 0)
    //А цена расчитывается от цены поставщика SYMMETRON  
    public sealed class PrikatProskitReportTemplate : PrikatReportTemplateBase
    {
        private readonly int SYMMETRON_STOCK_ID = 4;

        public PrikatProskitReportTemplate(string manufacturer, CurrencyType currency) : base(manufacturer, currency) { }

        protected override decimal LineDiscountForProductId(int product_id)
        {
            if (ProductIdToDiscount != null && ProductIdToDiscount.ContainsKey(product_id))
            {
                return ProductIdToDiscount[product_id];
            }
            return Discount1;
        }

        protected override void WriteProductLine(ProductEntity product, StreamWriter sw)
        {
            decimal priceInCurrency = product.stock_data == null ? 0 :
                product.stock_data.FirstOrDefault(s => s.stock_partner_id == SYMMETRON_STOCK_ID)?.original_price ?? 0;
            if (priceInCurrency == decimal.Zero)
            {
                return;
            }
            priceInCurrency = Math.Round(priceInCurrency / CurrencyRatio, 2);
            decimal purchasePrice = Math.Round(priceInCurrency * (100m + LineDiscountForProductId(product.product_id)) / 100m, Precission);
            string purchasePriceString = purchasePrice.ToString($"F{Precission}", new CultureInfo("en-EN"));

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
            WriteCell(sw, "0"); //Рекомендованная цена
            WriteCell(sw, purchasePriceString); //Закупочная цена
            WriteCell(sw, product.quantity.ToString()); //Количество остатков на скаладе
            WriteCell(sw, Currency.ToString().ToLower()); //Рекомендованная валюта
            WriteCell(sw, Currency.ToString().ToLower()); //Закупчная валюта
            sw.WriteLine();
        }
    }
}
