using System;
using System.ComponentModel;

namespace EtkBlazorApp.DataAccess.Model
{
    public class LogEntryEntity
    {
        public int id { get; set; }
        public LogEntryGroupName group_name { get; set; }
        public string user { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public DateTime date_time { get; set; }

    }

    public enum LogEntryGroupName
    {
        [Description("Не указано")]
        None,

        [Description("Приложение")]
        Auth,

        [Description("Цены и остатки")]
        PriceUpdate,

        [Description("Производитель")]
        ManufacturerUpdate,

        [Description("Обновление товара по ссылке")]
        ProductUpdate,

        [Description("Озон")]
        Ozon,

        [Description("ВсеИнструменты")]
        VseInstrumenti
    }
}
