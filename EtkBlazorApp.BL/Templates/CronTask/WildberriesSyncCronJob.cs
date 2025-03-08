using EtkBlazorApp.BL.Managers;
using EtkBlazorApp.DataAccess.Entity;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL.Templates.CronTask;

public class WildberriesCronTask : CronTask
{
    private readonly WildberriesUpdateService updateService;

    public WildberriesCronTask(WildberriesUpdateService updateService)
    {
        this.updateService = updateService;
    }

    public async Task Run(CronTaskEntity taskInfo, bool forced)
    {
        await updateService.UpdateWildberriesProducts(null);
    }
}
