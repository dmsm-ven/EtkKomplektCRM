using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class UserLogger 
    {
        private readonly ILogStorage logStorage;
        private readonly AuthenticationStateProvider stateProvider;

        public UserLogger(ILogStorage logStorage, AuthenticationStateProvider stateProvider)
        {
            this.logStorage = logStorage;
            this.stateProvider = stateProvider;
        }

        public async Task Write(LogEntryGroupName group, string title, string message)
        {
            var userLogin = (await stateProvider.GetAuthenticationStateAsync()).User.Identity.Name;

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
