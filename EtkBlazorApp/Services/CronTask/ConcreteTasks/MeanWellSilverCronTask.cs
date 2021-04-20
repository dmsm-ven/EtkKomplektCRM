using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using System.IO;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MeanWellSilverCronTask : CronTaskBase
    {
        public MeanWellSilverCronTask(CronTaskService service) : base(service, CronTaskPrefix.Silver) { }

        protected override async Task Run()
        {
            var templateType = typeof(MeanWellPriceListTemplate);
            var templateGuid = GetTemplateGuid(templateType);
            var templateInfo = await service.templates.GetPriceListTemplateById(templateGuid);

            IRemoteTemplateFileLoader loader = service.remoteTemplateLoaderFactory.GetMethod(templateInfo.remote_uri, templateInfo.remote_uri_method, templateGuid);
            using (var ms = new MemoryStream(await loader.GetBytes()))
            {
                var lines = await service.priceListManager.ReadTemplateLines(templateType, ms);
                await service.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
            }
        }
    }
}
