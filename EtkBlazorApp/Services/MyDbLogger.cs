using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Model;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace EtkBlazorApp.Services
{
    public class MyDbLogger : IAsyncDisposable
    {
        private readonly IDatabase db;
        private readonly Queue<LogEntryEntity> queue;
        private readonly Timer timer;

        public AuthenticationState AuthenticationState { get; set; }

        public TimeSpan SendEnterval { get; } = TimeSpan.FromSeconds(10);

        public MyDbLogger(IDatabase db)
        {
            this.db = db;
            queue = new Queue<LogEntryEntity>();
            timer = new Timer(SendEnterval.TotalMilliseconds);
            timer.Elapsed += async (o, e) => await Send();
            timer.Start();
        }

        public void Write(LogEntryGroupName group, string title, string message, string user = null)
        {
            queue.Enqueue(new LogEntryEntity() 
            { 
                User = (user ?? AuthenticationState?.User?.Identity?.Name) ?? "Нет данных", 
                DateTime = DateTime.Now, 
                GroupName = group, 
                Title = title, 
                Message = message
            });
        }

        public async Task Flush()
        {
            await db.AddLogEntries(queue.ToList());
        }

        private async Task Send()
        {
            if (queue.Count > 0)
            {
                await db.AddLogEntries(queue.ToList());
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Send();
        }
    }
}
