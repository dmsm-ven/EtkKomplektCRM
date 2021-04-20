using EtkBlazorApp.BL;
using System.Net.Http;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class PristCronTask : CronTaskBase
    {
        public PristCronTask(CronTaskService service) : base(service, CronTaskPrefix.Prist) { }

        protected override async Task Run()
        {
            var templateType = typeof(PristPriceListTemplate);
            var templateGuid = PriceListManager.GetPriceListGuidByType(templateType);
            var templateInfo = await service.templates.GetPriceListTemplateById(templateGuid);

            var response = await (new HttpClient().GetAsync(templateInfo.remote_uri));
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var lines = await service.priceListManager.ReadTemplateLines(templateType, stream);
                await service.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
            }
        }
    }
}
