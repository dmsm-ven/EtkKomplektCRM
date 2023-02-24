using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Notifiers;

public class MailkitOrderEmailNotificator : ICustomerOrderNotificator
{
    private readonly string settings_key = "customer_email_notificator";
    private readonly ISettingStorageReader settings;
    private readonly SystemEventsLogger logger;
    private readonly EncryptHelper encryptHelper;

    public MailkitOrderEmailNotificator(ISettingStorageReader settings, SystemEventsLogger logger, EncryptHelper encryptHelper)
    {
        this.settings = settings;
        this.logger = logger;
        this.encryptHelper = encryptHelper;
    }

    public async Task NotifyCustomer(long order_id, string customerEmail)
    {
        try
        {
            var configuration = new EmailNotificatorConfiguration()
            {
                Host = await settings.GetValue($"{settings_key}_{nameof(EmailNotificatorConfiguration.Host)}"),
                Login = await settings.GetValue($"{settings_key}_{nameof(EmailNotificatorConfiguration.Login)}"),
                Password = encryptHelper.Decrypt(await settings.GetValue($"{settings_key}_{nameof(EmailNotificatorConfiguration.Password)}")),
                Port = await settings.GetValue<int>($"{settings_key}_{nameof(EmailNotificatorConfiguration.Port)}"),
            };

            if (string.IsNullOrWhiteSpace(customerEmail) || order_id == 0 || !configuration.IsValid)
            {
                throw new FormatException();
            }

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(configuration.Login));
            email.To.Add(MailboxAddress.Parse(customerEmail));
            email.Subject = "ЕТК-Комплект. Заказ прибыл";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = BuildEmailBody(order_id)
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(configuration.Host, configuration.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(configuration.Login, configuration.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(quit: true);

            await logger.WriteSystemEvent(LogEntryGroupName.Orders, "Оповещение клиента", $"Уведомление о прибытии заказа № {order_id} отправлено на почту клиента: {customerEmail}");
        }
        catch (Exception ex)
        {
            await logger.WriteSystemEvent(LogEntryGroupName.Orders, "Ошибка уведомления", "Не удалось уведомить клиента об изменении статуса заказа. " + ex.Message);
        }
    }

    private string BuildEmailBody(long order_id)
    {
        return $"Ваш заказ №{order_id} прибыл в пункт выдачи. Если вы не делали заказ в ООО \"ЕТК-Комплект\" не обращайте внимания на это письмо";
    }
}

internal class EmailNotificatorConfiguration
{
    public string Host { get; init; }
    public string Login { get; init; }
    public string Password { get; init; }
    public int Port { get; init; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password) && Port != 0;
}
