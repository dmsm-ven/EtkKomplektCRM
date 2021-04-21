using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Templates.PriceListTemplates;
using System.IO;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MeanWellSilverCronTask : CronTaskByPriceListBase
    {
        public MeanWellSilverCronTask(CronTaskService service) : base(typeof(MeanWellPriceListTemplate), service, CronTaskPrefix.Silver)
        {

        }
    }





}
