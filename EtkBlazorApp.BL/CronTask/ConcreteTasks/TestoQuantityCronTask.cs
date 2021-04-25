using EtkBlazorApp.BL.Templates.PriceListTemplates;

namespace EtkBlazorApp.BL.CronTask
{
    public class TestoQuantityCronTask : CronTaskByPriceListBase
    {
        public TestoQuantityCronTask(CronTaskService service) : base(typeof(TestoQuantityPriceListTemplate), service, CronTaskPrefix.Testo_Quantity)
        {

        }
    }
}
