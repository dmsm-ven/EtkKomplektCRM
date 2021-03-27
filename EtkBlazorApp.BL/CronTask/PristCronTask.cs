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
            var id = ((PriceListTemplateDescriptionAttribute)typeof(PristPriceListTemplate)
                .GetCustomAttributes(typeof(PriceListTemplateDescriptionAttribute), false)
                .FirstOrDefault())
                .Guid;

            var templateInfo = await Manager.templates.GetPriceListTemplateById(id);

            //TODO тут и в другие местах где напрямую вызов HTTP клиент возможно надо сделать внедрение зависимостей
            var response = await (new HttpClient().GetAsync(templateInfo.remote_uri));
            using (var stream = await response.Content.ReadAsStreamAsync()) 
            { 
                long fileLength = (long)response.Content.Headers.ContentLength;
                var lines = await Manager.priceListManager.ReadTemplateLines(typeof(PristPriceListTemplate), stream, fileLength);
                await Manager.updateManager.UpdatePriceAndStock(lines, clearStockBeforeUpdate: false);
            }
            
        }
    }
}
