using EtkBlazorApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EtkBlazorApp.Core.Data.Cdek;
using EtkBlazorApp.BL;
using System.Threading.Tasks;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Core.Data.Order;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace EtkBlazorApp.Controllers
{
    [ApiController]
    [Route("api/cdek_webhook")]
    public class CdekWebhookHandlerController : Controller
    {
        private readonly IEtkUpdatesNotifier notifier;
        private readonly IOrderStorage orderStorage;
        private readonly ISettingStorageReader settings;
        private readonly IOrderUpdateService orderUpdateService;
        private readonly ICustomerOrderNotificator customerNotificator;
        private readonly SystemEventsLogger eventsLogger;

        public CdekWebhookHandlerController(IEtkUpdatesNotifier notifier,
            IOrderStorage orderStorage,
            ISettingStorageReader settings,
            IOrderUpdateService orderUpdateService,
            ICustomerOrderNotificator customerNotificator,
            SystemEventsLogger eventsLogger)
        {
            this.orderStorage = orderStorage ?? throw new ArgumentNullException(nameof(orderStorage));
            this.orderUpdateService = orderUpdateService ?? throw new ArgumentNullException(nameof(orderStorage));
            this.settings = settings;
            this.customerNotificator = customerNotificator;
            this.eventsLogger = eventsLogger;
            this.notifier = notifier;
        }

        //TODO: Сделать реальную проверку        
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] CdekWebhookOrderStatusData data)
        {
            string cdekHost = await settings.GetValue<string>("cdek_webhook_host");
            var isAuthorized = HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var ip) && ip == cdekHost;

            if (!isAuthorized)
            {
                await eventsLogger.WriteSystemEvent(LogEntryGroupName.Orders, "СДЭК", $"Неавторизованный доступ к webhook от хоста: {ip}");
                return Forbid();
            }

            string cdekOrderNumber = data?.attributes?.cdek_number;
            if (string.IsNullOrWhiteSpace(cdekOrderNumber))
            {
                return BadRequest();
            }

            OrderEntity shopOrder = await orderStorage.GetOrderByTkOrderNumber(cdekOrderNumber);

            CdekOrderStatusCode cdekStatus = data.attributes.GetCodeStatus();
            EtkOrderStatusCode orderStatus = cdekStatus switch
            {
                CdekOrderStatusCode.RECEIVED_AT_SHIPMENT_WAREHOUSE => EtkOrderStatusCode.InDelivery,
                CdekOrderStatusCode.DELIVERED => EtkOrderStatusCode.Completed,
                CdekOrderStatusCode.NOT_DELIVERED => EtkOrderStatusCode.Canceled,
                CdekOrderStatusCode.ACCEPTED_AT_PICK_UP_POINT => EtkOrderStatusCode.WaitingToPickup,
                _ => EtkOrderStatusCode.None
            };

            //На етк меняем статус заказа только если он: вручен, не вручен, поступил в доставку
            if (shopOrder != null && orderStatus != EtkOrderStatusCode.None)
            {
                await orderUpdateService.ChangeOrderStatus(shopOrder.order_id, (int)orderStatus);
            }

            bool etkNotify = new[] {
                CdekOrderStatusCode.NOT_DELIVERED,
                CdekOrderStatusCode.DELIVERED,
                CdekOrderStatusCode.ACCEPTED_AT_PICK_UP_POINT
            }.Any(status => status == cdekStatus);

            if (etkNotify)
            {
                await notifier.NotifOrderStatusChanged(shopOrder?.order_id, cdekOrderNumber, cdekStatus.GetDescriptionAttribute());
            }

            //if(shopOrder != null && cdekStatus == CdekOrderStatusCode.ACCEPTED_AT_PICK_UP_POINT)
            //проверка
            if (true)
            {
                await customerNotificator?.NotifyCustomer(shopOrder.order_id, "painven@gmail.com");
            }

            return Ok();
        }
    }
}


