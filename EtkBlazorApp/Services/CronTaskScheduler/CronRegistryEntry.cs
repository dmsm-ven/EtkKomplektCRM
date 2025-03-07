using NCrontab;
using System;

namespace EtkBlazorApp.Services.CronTaskScheduler;

public sealed record CronRegistryEntry(Type Type, CrontabSchedule CrontabSchedule);
