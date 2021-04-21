using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MegeonCronTask : CronTaskByPriceListBase
    {
        public MegeonCronTask(CronTaskService service) : base(typeof(MegeonPriceListTemplate), service, CronTaskPrefix.Megeon)
        {

        }
    }
}
