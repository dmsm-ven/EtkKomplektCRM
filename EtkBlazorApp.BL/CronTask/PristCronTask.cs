using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public class PristCronTask : CronTaskBase
    {
        public PristCronTask() : base(CronTaskPrefix.Prist) { }

        protected override async Task Run()
        {
            var templateType = typeof(PristPriceListTemplate);
            var templateGuid = GetTemplateGuid(templateType);
            var templateInfo = await Manager.templates.GetPriceListTemplateById(templateGuid);

            var response = await (new HttpClient().GetAsync(templateInfo.remote_uri));
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var lines = await Manager.priceListManager.ReadTemplateLines(templateType, stream);
                await Manager.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
            }

        }
    }
}
