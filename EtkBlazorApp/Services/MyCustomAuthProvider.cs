using EtkBlazorApp.BL;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MyCustomAuthProvider : AuthenticationStateProvider
    {
        private readonly ILogStorage log;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IAuthStateProcessor auth;
        private readonly ProtectedLocalStorage storage;

        public MyCustomAuthProvider(IAuthStateProcessor auth, ILogStorage log, IHttpContextAccessor contextAccessor, ProtectedLocalStorage storage)
        {
            this.storage = storage;
            this.log = log;
            this.contextAccessor = contextAccessor;
            this.auth = auth;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var login = await storage.GetAsync<string>("user_login");
            var password = await storage.GetAsync<string>("user_password");
            if (login.Success && password.Success)
            {
                var state = await AuthenticateUser(new AppUser() 
                { 
                    Login = login.Value, 
                    Password = password.Value,
                    UserIP = contextAccessor.HttpContext.Connection?.RemoteIpAddress.ToString()
                });                
                return state;           
            }

            return GetDefaultState();            
        }

        public async Task<AuthenticationState> AuthenticateUser(AppUser userData)
        {           
            string permission = await auth.GetUserPermission(userData.Login, userData.Password);

            var logEntry = new LogEntryEntity()
            {
                group_name = LogEntryGroupName.Auth.GetDescriptionAttribute(),
                user = userData.Login,
                date_time = DateTime.Now
            };

            if (string.IsNullOrWhiteSpace(permission))
            {
                logEntry.title = "Ошибка";
                logEntry.message = $"Неудачная попытка входа";
                await log.Write(logEntry); 

                return GetDefaultState();
            }

            var userInfo = (await auth.GetUsers()).Single(u => u.login == userData.Login);

            if (!string.IsNullOrWhiteSpace(userInfo.ip) && userInfo.ip != userData.UserIP)
            {
                logEntry.title = "Ошибка";
                logEntry.message = $"Вход для IP {userData.UserIP} запрещен";
                await log.Write(logEntry);

                return GetDefaultState();
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userData.Login),
                new Claim(ClaimTypes.Role, permission)
            }, "login_form");
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            await auth.UpdateUserLastLoginDate(userData.Login);

            logEntry.title = "Вход";
            logEntry.message = $"Пользователь успешно залогинился";
            await log.Write(logEntry);

            return state;
        }  
        
        public async Task LogOutUser()
        {            
            string userName = (await storage.GetAsync<string>("user_login")).Value ?? string.Empty;
          
            await storage.DeleteAsync("user_login");
            await storage.DeleteAsync("user_password");

            NotifyAuthenticationStateChanged(Task.FromResult(GetDefaultState()));

            var logEntry = new LogEntryEntity()
            {
                group_name = LogEntryGroupName.Auth.GetDescriptionAttribute(),
                user = userName,
                title = "Выход",
                message = "Пользователь разлогинился",
                date_time = DateTime.Now
            };
            await log.Write(logEntry);
        }

        private AuthenticationState GetDefaultState()
        {
            var identity = new ClaimsIdentity(new[]
{
                new Claim(ClaimTypes.Name, "Гость")
            }, authenticationType: string.Empty);
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            var state = new AuthenticationState(user);
            return state;
        }
    }
}
