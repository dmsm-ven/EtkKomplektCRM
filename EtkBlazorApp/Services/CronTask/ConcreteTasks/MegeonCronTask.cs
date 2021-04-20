using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MegeonCronTask : CronTaskBase
    {
        public MegeonCronTask(CronTaskService service) : base(service, CronTaskPrefix.Megeon) { }

        protected override async Task Run()
        {
            var templateType = typeof(MegeonPriceListTemplate);
            var templateGuid = GetTemplateGuid(templateType);
            var templateInfo = await service.templates.GetPriceListTemplateById(templateGuid);

            IRemoteTemplateFileLoader loader = service.remoteTemplateLoaderFactory.GetMethod(templateInfo.remote_uri);
            using (var ms = new MemoryStream(await loader.GetBytes()))
            {
                var lines = await service.priceListManager.ReadTemplateLines(templateType, ms);
                await service.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
            }
        }
    }
}
