using EtkBlazorApp.BL.Templates.PriceListTemplates;

namespace EtkBlazorApp.BL.CronTask
{
    public class MeanWellSilverCronTask : CronTaskByPriceListBase
    {
        public MeanWellSilverCronTask(CronTaskService service) 
            : base(typeof(MeanWellSilverPriceListTemplate), service, CronTaskPrefix.MeanWell_Silver)
        {

        }
    }

    public class MeanWellPartnerCronTask : CronTaskByPriceListBase
    {
        public MeanWellPartnerCronTask(CronTaskService service) 
            : base(typeof(MeanWellPartnerPriceListTemplate), service, CronTaskPrefix.MeanWell_Partner)
        {

        }
    }
}
