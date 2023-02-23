using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.Core.Data.Cdek;

/// <summary>
/// Статусы заказов СДЭК. Полный список статусов на странице: https://api-docs.cdek.ru/29924139.html
/// </summary>
public enum CdekOrderStatusCode
{
    [Description("Не указано")]
    None,

    [Description("Создан")]
    CREATED,

    [Description("Удален")]
    REMOVED,

    [Description("Вручен")]
    DELIVERED, // Успешно доставлен и вручен адресату (конечный статус)

    [Description("Не вручен")]
    NOT_DELIVERED, // 	Покупатель отказался от покупки, возврат в ИМ (конечный статус),

    [Description("Принят")]
    RECEIVED_AT_SHIPMENT_WAREHOUSE, //Оформлен приход на склад СДЭК в городе-отправителе.

    [Description("Прибыл в пункт")]
    ACCEPTED_AT_PICK_UP_POINT, //Оформлен приход на склад СДЭК в городе-отправителе.
}
