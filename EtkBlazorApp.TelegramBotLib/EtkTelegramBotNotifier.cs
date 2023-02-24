using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace EtkBlazorApp.TelegramBotLib;

public class EtkTelegramBotNotifier : IEtkUpdatesNotifier
{
    private readonly string ETK_LK_HOST = "https://lk.etk-komplekt.ru";
    private readonly string CDEK_LK_HOST = "https://lk.cdek.ru";
    private readonly ITelegramBotClient bot;
    private readonly IEtkUpdatesNotifierMessageFormatter messageFormatter;
    private readonly ISettingStorageReader settings;
    private readonly long ChannelId;

    public async Task<bool> IsActive() => await settings.GetValue<bool>("telegram_notification_enabled");

    public EtkTelegramBotNotifier(IEtkUpdatesNotifierMessageFormatter messageFormatter,
        ISettingStorageReader settings,
        string token, long channelId)
    {
        this.messageFormatter = messageFormatter ?? throw new ArgumentNullException(nameof(messageFormatter));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

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
        bool generalStatus = await IsActive();
        bool isNotiyfyPriceChangedEnabled = await settings.GetValue<bool>("telegram_notification_price_enabled");

        if (!generalStatus || !isNotiyfyPriceChangedEnabled)
        {
            return;
        }

        string message = messageFormatter.GetPriceListChangedMessage(data.PriceListName, data.MinimumOverpricePercent, data.Data.Count);

        var replyMarkup = GetSimpleMarkupWithUri($"{ETK_LK_HOST}/price-list/products-price-history/{data.PriceListGuid}");

        await bot.SendTextMessageAsync(ChannelId, message, ParseMode.Html, replyMarkup: replyMarkup);
    }

    /// <summary>
    /// Уведомляем о неудачной загрузке прайс-листа
    /// </summary>
    /// <param name="taskName"></param>
    /// <returns></returns>
    public async Task NotifyPriceListLoadingError(string taskName)
    {
        bool generalStatus = await IsActive();
        var taskErrorEbaled = await settings.GetValue<bool>("telegram_notification_task_enabled");

        if (!generalStatus || !taskErrorEbaled)
        {
            return;
        }

        string message = messageFormatter.GetTaskLoadErrorMessage(taskName);

        var replyMarkup = GetSimpleMarkupWithUri($"{ETK_LK_HOST}/cron-task-history");

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
        bool generalStatus = await IsActive();
        bool cdekOrderStatusChangedEnabled = await settings.GetValue<bool>("telegram_notification_cdek_enabled");

        if (!generalStatus || !cdekOrderStatusChangedEnabled)
        {
            return;
        }

        string message = messageFormatter.GetOrderStatusChangedMessage(etkOrderId, cdekOrderId, statusName);
        string buttonUrl = etkOrderId.HasValue ?
            $"{ETK_LK_HOST}/order/{etkOrderId.Value}" :
            $"{CDEK_LK_HOST}/order-history/{cdekOrderId}/view";

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
