using EtkBlazorApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EtkBlazorApp.Core.Data.Cdek;
using EtkBlazorApp.BL;
using System.Threading.Tasks;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Core.Data.Order;

namespace EtkBlazorApp.Controllers
{
    [ApiController]
    [Route("api/cdek_webhook")]
    public class CdekWebhookHandlerController : Controller
    {
        private readonly ITransportCompanyApi cdekApi;
        private readonly IEtkUpdatesNotifier notifier;
        private readonly IOrderStorage orderStorage;
        private readonly IOrderUpdateService orderUpdateService;
        private readonly SystemEventsLogger eventsLogger;

        public CdekWebhookHandlerController(ITransportCompanyApi cdekApi,
            IEtkUpdatesNotifier notifier,
            IOrderStorage orderStorage,
            IOrderUpdateService orderUpdateService,
            SystemEventsLogger eventsLogger)
        {
            this.cdekApi = cdekApi;
            this.notifier = notifier;
            this.eventsLogger = eventsLogger;
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
                return BadRequest();
            }

            var status = data.attributes.GetCodeStatus();

            //Берем только конечные статусы, другие не нужны
            if (status == CdekOrderStatusCode.DELIVERED || status == CdekOrderStatusCode.NOT_DELIVERED)
            {
                string statusName = (status == CdekOrderStatusCode.DELIVERED ? 
                    "Вручен" : 
                    "Не вручен");
                int orderStatus = (status == CdekOrderStatusCode.DELIVERED ? 
                    (int)OrderStatusCode.Completed : 
                    (int)OrderStatusCode.Canceled);
                string message = $"Заказ номер {shopOrder.order_id} (№ СДЭК {data.attributes.cdek_number}) {statusName}";

                await orderUpdateService.ChangeOrderStatus(shopOrder.order_id, orderStatus);              
                await eventsLogger.WriteSystemEvent(LogEntryGroupName.Orders, "СДЭК", message);
            }
            return Ok();
        }
    }
}


