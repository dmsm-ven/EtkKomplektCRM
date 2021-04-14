using System.ComponentModel;

namespace EtkBlazorApp.BL
{
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

        [Description("Обновление шаблона")]
        TemplateUpdate,

        [Description("Задание")]
        CronTask,

        [Description("Аккаунты")]
        Accounts


    }
}
