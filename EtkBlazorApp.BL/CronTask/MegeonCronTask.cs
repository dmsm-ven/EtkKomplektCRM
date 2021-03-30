using EtkBlazorApp.BL.Templates.PriceListTemplates;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class MegeonCronTask : CronTaskBase
    {
        public MegeonCronTask() : base(CronTaskPrefix.Megeon) { }

        protected override async Task Run()
        {
            var templateType = typeof(MegeonPriceListTemplate);
            var templateGuid = GetTemplateGuid(templateType);
            var templateInfo = await Manager.templates.GetPriceListTemplateById(templateGuid);

            string login = await Manager.settings.GetValue($"price-list-template-credentials-{templateInfo.title}-login");
            string password = await Manager.settings.GetValue($"price-list-template-credentials-{templateInfo.title}-password");

            using (var wc = new WebClient())
            {
                wc.Credentials = new NetworkCredential(login, password);
                var bytes = await Task.Run(() => wc.DownloadData(templateInfo.remote_uri));
                using (var ms = new MemoryStream(bytes))
                {
                    var lines = await Manager.priceListManager.ReadTemplateLines(templateType, ms);
                    await Manager.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
                }
            }

        }
    }
}
