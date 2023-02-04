using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using System.Text;

namespace EtkBlazorApp.TelegramBotLib;

public class TelegramNotifierMessageFormatter : IEtkUpdatesNotifierMessageFormatter
{
    public string GetOrderStatusChangedMessage(int order_id, string statusName)
    {
        string message = $"🚚📦 Статус заказа <b>{order_id}</b> измен на <b>{statusName}</b>";
        return message;
    }

    public string GetPriceListChangedMessage(PriceListProductPriceChangeHistory data)
    {
        var message = new StringBuilder()
            .Append($"🔎 При загрузке прайс-листа <b>{data.PriceListName}</b>\n")
            .Append($"Обнаружено повышение цен 📈, более чем на <b>{data.MinimumOverpricePercent:P0}</b>\n")
            .Append($"В <b>{data.Data.Count}</b> 📦 товарах ")
            .ToString();

        return message;
    }

    public string GetTaskLoadErrorMessage(string taskName)
    {
        string message = $"🔥 Выполнение 🕒 задачи <b>{taskName}</b> завершилось ошибкой 🛑";
        return message;
    }
}
