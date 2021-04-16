using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class UserLogger 
    {
        private readonly ILogStorage logStorage;
        private readonly AuthenticationStateProvider userPrivder;

        public UserLogger(ILogStorage logStorage, AuthenticationStateProvider userPrivder)
        {
            this.logStorage = logStorage;
            this.userPrivder = userPrivder;
        }

        public async Task Write(LogEntryGroupName group, string title, string message)
        {
            var user = (await userPrivder.GetAuthenticationStateAsync()).User.Identity.Name;
            var entity = new LogEntryEntity()
            {
                date_time = DateTime.Now,
                user = user,
                title = title,
                group_name = group.GetDescriptionAttribute(),
                message = message
            };

            await logStorage.Write(entity);
        }
    }

    public class SystemEventsLogger 
    {
        private readonly ILogStorage logStorage;

        public SystemEventsLogger(ILogStorage logStorage)
        {
            this.logStorage = logStorage;
        }

        public async Task WriteSystemEvent(LogEntryGroupName group, string title, string message)
        {
            var entity = new LogEntryEntity()
            {
                date_time = DateTime.Now,
                user = "System",
                title = title,
                group_name = group.GetDescriptionAttribute(),
                message = message
            };
            await logStorage.Write(entity);
        }
    }
}
