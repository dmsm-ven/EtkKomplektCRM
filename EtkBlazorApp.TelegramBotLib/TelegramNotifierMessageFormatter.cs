using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using System.Text;

namespace EtkBlazorApp.TelegramBotLib;

public class TelegramNotifierMessageFormatter : IEtkUpdatesNotifierMessageFormatter
{
    public string GetOrderStatusChangedMessage(int? etkOrderId, string cdekOrderId, string statusName)
    {
        string message = null;

        if (etkOrderId.HasValue)
        {
            message = $"🚚📦 Статус заказа ЕТК №<b>{etkOrderId.Value}</b> (СДЭК №<b>{cdekOrderId}</b>) измен на <b>{statusName}</b>";
        }
        else
        {
            message = $"🚚📦 Статус заказа СДЭК №<b>{cdekOrderId}</b> измен на <b>{statusName}</b>";
        }

        return message;
    }

    public string GetPriceListChangedMessage(string priceListName, double percent, int totalProducts)
    {
        var message = new StringBuilder()
            .Append($"🔎 При загрузке прайс-листа <b>{priceListName}</b>\n")
            .Append($"Обнаружено повышение цен 📈, более чем на <b>{percent:P0}</b>\n")
            .Append($"В <b>{totalProducts}</b> 📦 товарах ")
            .ToString();

        return message;
    }

    public string GetTaskLoadErrorMessage(string taskName)
    {
        string message = $"🔥 Выполнение 🕒 задачи <b>{taskName}</b> завершилось ошибкой 🛑";
        return message;
    }
}
