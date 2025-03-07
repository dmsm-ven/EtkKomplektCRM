using Microsoft.Extensions.Logging;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services.CronTaskScheduler.CronJobs;

public class TestCronJob : ICronJob
{
    private static readonly Logger nlog = LogManager.GetCurrentClassLogger();

    private readonly ILogger<TestCronJob> _logger;

    public TestCronJob(ILogger<TestCronJob> logger)
    {
        _logger = logger;
        nlog.Info("TestCronJob CREATED");
    }
    public Task Run(CancellationToken token = default)
    {
        nlog.Info("TestCronJob TICK");

        return Task.CompletedTask;
    }
}
