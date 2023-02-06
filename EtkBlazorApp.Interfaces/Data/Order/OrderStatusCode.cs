using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.Core.Data.Order;

public enum EtkOrderStatusCode
{
    [Description("Неизвестно")]
    None = 0,

    [Description("Создан")]
    Created = 1,

    [Description("В обработке")]
    InProcessing = 2,

    [Description("Отменен")]
    Canceled = 9,

    [Description("В доставке")]
    InDelivery = 17,

    [Description("Ожидает получения")]
    WaitingToPickup = 18,

    [Description("Выполнен")]
    Completed = 19,

    [Description("Оплачен ЮKassa")]
    PaidUsingYKassa = 20,
}
