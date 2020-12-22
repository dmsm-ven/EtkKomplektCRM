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
        private readonly List<LogEntryEntity> items;
        private readonly Timer timer;

        public AuthenticationState AuthenticationState { get; set; }

        public TimeSpan SendEnterval { get; } = TimeSpan.FromSeconds(10);

        public MyDbLogger(IDatabase db)
        {
            this.db = db;
            items = new List<LogEntryEntity>();
            timer = new Timer(SendEnterval.TotalMilliseconds);
            timer.Elapsed += async (o, e) => await Flush();
            timer.Start();
        }

        public void Write(LogEntryGroupName group, string title, string message, string user = null)
        {
            items.Add(new LogEntryEntity() 
            { 
                user = (user ?? AuthenticationState?.User?.Identity?.Name) ?? "Нет данных", 
                date_time = DateTime.Now, 
                group_name = group, 
                title = title, 
                message = message
            });
        }

        public async Task Flush()
        {
            if (items.Count > 0)
            {
                await db.AddLogEntries(items);
                items.Clear();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Flush();
        }
    }
}
