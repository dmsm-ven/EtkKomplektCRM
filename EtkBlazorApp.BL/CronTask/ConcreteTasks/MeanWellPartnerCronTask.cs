using EtkBlazorApp.BL.Templates.PriceListTemplates;

namespace EtkBlazorApp.BL.CronTask
{
    public class MeanWellPartnerCronTask : CronTaskByPriceListBase
    {
        public MeanWellPartnerCronTask(CronTaskService service) 
            : base(typeof(MeanWellPartnerPriceListTemplate), service, CronTaskPrefix.MeanWell_Partner)
        {

        }
    }
}
