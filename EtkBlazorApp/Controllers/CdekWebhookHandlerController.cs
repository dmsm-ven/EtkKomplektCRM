using EtkBlazorApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using EtkBlazorApp.Core.Data.Cdek;
using EtkBlazorApp.BL;
using System.Threading.Tasks;

namespace EtkBlazorApp.Controllers
{
    [ApiController]
    [Route("api/cdek_webhook")]
    public class CdekWebhookHandlerController : Controller
    {
        private readonly ITransportCompanyApi cdekApi;
        private readonly IEtkUpdatesNotifier notifier;
        private readonly SystemEventsLogger eventsLogger;

        public CdekWebhookHandlerController(ITransportCompanyApi cdekApi,
            IEtkUpdatesNotifier notifier,
            SystemEventsLogger eventsLogger)
        {
            this.cdekApi = cdekApi;
            this.notifier = notifier;
            this.eventsLogger = eventsLogger;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] CdekWebhookOrderStatusData data)
        {
            if (data.attributes != null)
            {
                string message = $"Статус СДЭК-заказа {data.attributes.cdek_number} изменен на {data.attributes.code}";

                await eventsLogger.WriteSystemEvent(LogEntryGroupName.Orders, "СДЭК", message);
            }
            return Ok();
        }
    }
}


