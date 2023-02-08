using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace EtkBlazorApp.TelegramBotLib;

public class EtkTelegramBotNotifier : IEtkUpdatesNotifier
{
    private readonly ITelegramBotClient bot;
    private readonly IEtkUpdatesNotifierMessageFormatter messageFormatter;
    private readonly long ChannelId;

    public EtkTelegramBotNotifier(IEtkUpdatesNotifierMessageFormatter messageFormatter, string token, long channelId)
    {
        this.messageFormatter = messageFormatter ?? throw new ArgumentNullException(nameof(messageFormatter));

        if (channelId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channelId));
        }
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException(nameof(token));
        }

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
        string message = messageFormatter.GetPriceListChangedMessage(data.PriceListName, data.MinimumOverpricePercent, data.Data.Count);

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
        string message = messageFormatter.GetTaskLoadErrorMessage(taskName);

        var replyMarkup = GetSimpleMarkupWithUri("https://lk.etk-komplekt.ru/cron-task-history");

        await bot.SendTextMessageAsync(ChannelId, message, ParseMode.Html, replyMarkup: replyMarkup);
    }

    /// <summary>
    /// Уведомляем об изменении статуса заказа
    /// </summary>
    /// <param name="order_id"></param>
    /// <param name="statusName"></param>
    /// <returns></returns>
    public async Task NotifOrderStatusChanged(int? etkOrderId, string cdekOrderId, string statusName)
    {
        string message = messageFormatter.GetOrderStatusChangedMessage(etkOrderId, cdekOrderId, statusName);

        string buttonUrl = etkOrderId.HasValue ?
            $"https://lk.etk-komplekt.ru/order/{etkOrderId.Value}" :
            $"https://lk.cdek.ru/order-history/{cdekOrderId}/view";

        InlineKeyboardMarkup replyMarkup = GetSimpleMarkupWithUri(buttonUrl);

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
