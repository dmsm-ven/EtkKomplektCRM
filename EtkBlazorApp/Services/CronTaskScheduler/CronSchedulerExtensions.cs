using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NCrontab;
using System;

namespace EtkBlazorApp.Services.CronTaskScheduler;

public static class CronSchedulerExtensions
{
    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, string cronExpression)
        where T : class, ICronJob
    {
        var cron = CrontabSchedule.TryParse(cronExpression)
                   ?? throw new ArgumentException("Invalid cron expression", nameof(cronExpression));

        var entry = new CronRegistryEntry(typeof(T), cron);

        services.AddHostedService<CronScheduler>();
        services.TryAddSingleton<T>();
        services.AddSingleton(entry);

        return services;
    }
}
