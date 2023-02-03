using EtkBlazorApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EtkBlazorApp.Core.Data.Cdek;
using EtkBlazorApp.BL;
using System.Threading.Tasks;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Core.Data.Order;
using System;

namespace EtkBlazorApp.Controllers
{
    [ApiController]
    [Route("api/cdek_webhook")]
    public class CdekWebhookHandlerController : Controller
    {
        private readonly IEtkUpdatesNotifier notifier;
        private readonly IOrderStorage orderStorage;
        private readonly IOrderUpdateService orderUpdateService;
        private readonly SystemEventsLogger eventsLogger;

        public CdekWebhookHandlerController(IEtkUpdatesNotifier notifier,
            IOrderStorage orderStorage,
            IOrderUpdateService orderUpdateService,
            SystemEventsLogger eventsLogger)
        {
            this.orderStorage = orderStorage ?? throw new ArgumentNullException(nameof(orderStorage));
            this.orderUpdateService = orderUpdateService ?? throw new ArgumentNullException(nameof(orderStorage));
            this.eventsLogger = eventsLogger;
            this.notifier = notifier;
        }

        //TODO: Добавить проверку что заказ действительно от сдэка
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] CdekWebhookOrderStatusData data)
        {
            if (string.IsNullOrWhiteSpace(data?.attributes?.cdek_number))
            {
                return BadRequest();
            }

            OrderEntity shopOrder = await orderStorage.GetOrderByCdekNumber(data.attributes.cdek_number);
            if (shopOrder == null)
            {
                return Ok();
            }

            //Берем только конечные статусы, другие не нужны
            var cdekStatus = data.attributes.GetCodeStatus();
            if (cdekStatus.IsEndpointStatus())
            {
                return Ok();
            }

            string statusName = (cdekStatus == CdekOrderStatusCode.DELIVERED ? "Вручен" : "Не вручен");
            OrderStatusCode orderStatus = (cdekStatus == CdekOrderStatusCode.DELIVERED ? OrderStatusCode.Completed : OrderStatusCode.Canceled);
            string message = $"Заказ ETK {shopOrder.order_id} (СДЭК {data.attributes.cdek_number}) {statusName}";

            await orderUpdateService.ChangeOrderStatus(shopOrder.order_id, (int)orderStatus);
            await eventsLogger?.WriteSystemEvent(LogEntryGroupName.Orders, "СДЭК", message);
            await notifier.NotifOrderStatusChanged(shopOrder.order_id, statusName);

            return Ok();
        }
    }
}


