using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services.CronTaskScheduler.CronJobs;

public class WildberriesSyncCronJob : ICronJob
{
    private readonly WildberriesUpdateService updateService;

    public WildberriesSyncCronJob(WildberriesUpdateService updateService)
    {
        this.updateService = updateService;
    }

    public async Task Run(CancellationToken token = default)
    {
        await updateService.UpdateWildberriesProducts(null);
    }
}
