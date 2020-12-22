using System;
using System.ComponentModel;

namespace EtkBlazorApp.DataAccess.Model
{
    public class LogEntryEntity
    {
        public int Id { get; set; }
        public LogEntryGroupName GroupName { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }

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
