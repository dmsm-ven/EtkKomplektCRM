using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace EtkBlazorApp.TelegramBotLib;

public class EtkTelegramBotNotifier : IEtkUpdatesNotifier
{
    private readonly ITelegramBotClient bot;
    private readonly long ChannelId;

    public EtkTelegramBotNotifier(string token, long channelId)
    {
        ChannelId = channelId;
        bot = new TelegramBotClient(token);
        bot.StartReceiving(HandleUpdate, HandleException);
    }

    /// <summary>
    /// Уведомляем об изменении в цене на прайс-лист
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task NotifyPriceListProductPriceChanged(PriceListProductPriceChangeHistory data)
    {
        var message = new StringBuilder()
            .Append($"🔎 При загрузке прайс-листа <b>{data.PriceListName}</b>\n")
            .Append($"Обнаружено повышение цен 📈, более чем на <b>{data.MinimumOverpricePercent:P0}</b>\n")
            .Append($"В <b>{data.Data.Count}</b> 📦 товарах ")
            .ToString();

        var replyMarkup = GetSimpleMarkupWithUri($"https://lk.etk-komplekt.ru/price-list/products-price-history/{data.PriceListGuid}");

        await bot.SendTextMessageAsync(ChannelId, message, ParseMode.Html, replyMarkup: replyMarkup);
    }

    /// <summary>
    /// Уведомляем о неудачной загрузке прайс-листа
    /// </summary>
    /// <param name="taskName"></param>
    /// <returns></returns>
    public async Task NotifyPriceListLoadingError(string taskName)
    {
        string message = $"🛑 Выполнение 🕒 задачи <b>{taskName}</b> завершилось ошибкой";

        var replyMarkup = GetSimpleMarkupWithUri("https://lk.etk-komplekt.ru/cron-task-history");

        await bot.SendTextMessageAsync(ChannelId, message, ParseMode.Html, replyMarkup: replyMarkup);
    }

    /// <summary>
    /// Уведомляем об изменении статуса заказа
    /// </summary>
    /// <param name="order_id"></param>
    /// <param name="statusName"></param>
    /// <returns></returns>
    public async Task NotifOrderStatusChanged(int order_id, string statusName)
    {
        string message = $"🚚📦 Статус заказа <b>{order_id}</b> измен на <b>{statusName}</b>";

        var replyMarkup = GetSimpleMarkupWithUri($"https://lk.etk-komplekt.ru/order/{order_id}");

        await bot.SendTextMessageAsync(ChannelId, message, ParseMode.Html, replyMarkup: replyMarkup);
    }

    private InlineKeyboardMarkup GetSimpleMarkupWithUri(string uri)
    {
        var lkButton = new InlineKeyboardButton("Смотреть в LK")
        {
            Url = uri
        };
        var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[] { lkButton });

        return replyMarkup;
    }

    private async void HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken cancel)
    {
        if (update.Message?.Chat != null)
        {
            await bot.SendTextMessageAsync(update.Message.Chat.Id, "Бот работает в одностороннем режиме");
        }
    }

    private void HandleException(ITelegramBotClient bot, Exception exception, CancellationToken cancel)
    {

    }
}
