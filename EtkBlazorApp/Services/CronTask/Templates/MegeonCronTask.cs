using EtkBlazorApp.BL.Templates.PriceListTemplates;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MegeonCronTask : CronTaskBase
    {
        public MegeonCronTask() : base(CronTaskPrefix.Megeon) { }

        protected override async Task Run()
        {
            var templateType = typeof(MegeonPriceListTemplate);
            var templateGuid = GetTemplateGuid(templateType);
            var templateInfo = await service.templates.GetPriceListTemplateById(templateGuid);

            string login = await service.settings.GetValue($"price-list-template-credentials-{templateInfo.title}-login");
            string password = await service.settings.GetValue($"price-list-template-credentials-{templateInfo.title}-password");

            using (var wc = new WebClient())
            {
                wc.Credentials = new NetworkCredential(login, password);
                var bytes = await Task.Run(() => wc.DownloadData(templateInfo.remote_uri));
                using (var ms = new MemoryStream(bytes))
                {
                    var lines = await service.priceListManager.ReadTemplateLines(templateType, ms);
                    await service.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
                }
            }

        }
    }
}
