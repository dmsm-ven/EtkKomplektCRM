using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.Core.Data.Cdek;

/// <summary>
/// Статусы заказов СДЭК. Полный список статусов на странице: https://api-docs.cdek.ru/29924139.html
/// </summary>
public enum CdekOrderStatusCode
{
    None,
    CREATED,
    REMOVED,
    DELIVERED, // Успешно доставлен и вручен адресату (конечный статус)
    NOT_DELIVERED, // 	Покупатель отказался от покупки, возврат в ИМ (конечный статус)
}
