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

namespace EtkBlazorApp.Controllers
{
    [ApiController]
    [Route("api/cdek_webhook")]
    public class CdekWebhookHandlerController : Controller
    {
        private readonly IEtkUpdatesNotifier notifier;
        private readonly IOrderStorage orderStorage;
        private readonly ISettingStorage settings;
        private readonly IOrderUpdateService orderUpdateService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly SystemEventsLogger eventsLogger;

        public CdekWebhookHandlerController(IEtkUpdatesNotifier notifier,
            IOrderStorage orderStorage,
            ISettingStorage settings,
            IOrderUpdateService orderUpdateService,
            IHttpContextAccessor httpContextAccessor,
            SystemEventsLogger eventsLogger)
        {
            this.orderStorage = orderStorage ?? throw new ArgumentNullException(nameof(orderStorage));
            this.settings = settings;
            this.orderUpdateService = orderUpdateService ?? throw new ArgumentNullException(nameof(orderStorage));
            this.httpContextAccessor = httpContextAccessor;
            this.eventsLogger = eventsLogger;
            this.notifier = notifier;
        }

        //TODO: Добавить проверку что заказ действительно от сдэка
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] CdekWebhookOrderStatusData data)
        {
            var invalidHost = !httpContextAccessor.HttpContext.Request.Host.HasValue ||
                !httpContextAccessor.HttpContext.Request.Host.Value.Contains("cdek.ru");

            if (invalidHost)
            {
                return BadRequest();
            }

            string cdekOrderNumber = data?.attributes?.cdek_number;
            if (string.IsNullOrWhiteSpace(cdekOrderNumber))
            {
                return Ok();
            }

            OrderEntity shopOrder = await orderStorage.GetOrderByCdekNumber(cdekOrderNumber);
            CdekOrderStatusCode cdekStatus = data.attributes.GetCodeStatus();
            EtkOrderStatusCode orderStatus = cdekStatus switch
            {
                CdekOrderStatusCode.RECEIVED_AT_SHIPMENT_WAREHOUSE => EtkOrderStatusCode.InDelivery,
                CdekOrderStatusCode.DELIVERED => EtkOrderStatusCode.Completed,
                CdekOrderStatusCode.NOT_DELIVERED => EtkOrderStatusCode.Canceled,
                _ => EtkOrderStatusCode.None
            };

            //На етк меняем статус заказа только если он: вручен, не вручен, поступил в доставку
            if (shopOrder != null && orderStatus != EtkOrderStatusCode.None)
            {
                await orderUpdateService.ChangeOrderStatus(shopOrder.order_id, (int)orderStatus);
            }

            if (cdekStatus == CdekOrderStatusCode.DELIVERED || cdekStatus == CdekOrderStatusCode.NOT_DELIVERED)
            {
                var generalStatus = await settings.GetValue<bool>("telegram_notification_enabled");
                var cdekOrderStatusChangedEnabled = await settings.GetValue<bool>("telegram_notification_cdek_enabled");
                if (generalStatus && cdekOrderStatusChangedEnabled)
                {
                    await notifier.NotifOrderStatusChanged(shopOrder?.order_id, cdekOrderNumber, cdekStatus.GetDescriptionAttribute());
                }
            }

            return Ok();
        }
    }
}


