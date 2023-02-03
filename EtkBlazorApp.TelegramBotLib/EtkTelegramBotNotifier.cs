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

    public async Task NotifyPriceListProductPriceChanged(PriceListProductPriceChangeHistory data)
    {
        var message = new StringBuilder()
            .Append($"🔎 При загрузке прайс-листа <b>{data.PriceListName}</b>\n")
            .Append($"Обнаружено повышение цен 📈, более чем на <b>{data.MinimumOverpricePercent:P0}</b>\n")
            .Append($"В <b>{data.Data.Count}</b> 📦 товарах ")
            .ToString();

        var lkButton = new InlineKeyboardButton("Открыть в личном кабинете")
        {
            Url = $"https://lk.etk-komplekt.ru/price-list/products-price-history/{data.PriceListGuid}"
        };
        var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[] { lkButton });

        await bot.SendTextMessageAsync(ChannelId, message, ParseMode.Html, replyMarkup: replyMarkup);
    }

    public async Task NotifyPriceListLoadingError(string taskName)
    {
        string message = $"🛑 Выполнение 🕒 задачи <b>{taskName}</b> завершилось ошибкой";

        var lkButton = new InlineKeyboardButton("Открыть журнал выполнения")
        {
            Url = $"https://lk.etk-komplekt.ru/cron-task-history"
        };
        var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[] { lkButton });

        await bot.SendTextMessageAsync(ChannelId, message, ParseMode.Html, replyMarkup: replyMarkup);
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
