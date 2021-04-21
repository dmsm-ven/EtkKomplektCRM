using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class UserLogger 
    {
        private readonly ILogStorage logStorage;
        private readonly AuthenticationStateProvider stateProvider;
        private string userLogin;

        public UserLogger(ILogStorage logStorage, AuthenticationStateProvider stateProvider)
        {
            this.logStorage = logStorage;
            this.stateProvider = stateProvider;

            stateProvider.AuthenticationStateChanged += async (e) =>
            {
                userLogin = (await e).User.Identity.Name;
            };
        }

        public async Task Write(LogEntryGroupName group, string title, string message)
        {
            var entity = new LogEntryEntity()
            {
                date_time = DateTime.Now,
                user = userLogin,
                title = title,
                group_name = group.GetDescriptionAttribute(),
                message = message
            };

            await logStorage.Write(entity);
        }
    }
}
