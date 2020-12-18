using EtkBlazorApp.BL.Data;
using EtkBlazorApp.DataAccess;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Serilog;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MyCustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IDatabase db;
        private readonly ProtectedLocalStorage storage;

        public MyCustomAuthProvider(IDatabase db, ProtectedLocalStorage storage)
        {
            this.db = db;
            this.storage = storage;
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
                    Password = password.Value }
                );
                return state;           
            }

            return GetDefaultState();            
        }

        public async Task<AuthenticationState> AuthenticateUser(AppUser userData)
        {
            var permission = await db.GetUserPermission(userData.Login, userData.Password);

            if(string.IsNullOrWhiteSpace(permission))
            {
                Log.Warning($"Не верный ввод пароль '{userData.Login}' | '{userData.Password}'");
                return GetDefaultState(); 
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userData.Login),
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(ClaimTypes.Role, "Manager"),
            }, "login_form");

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            Log.Information($"Пользователь '{userData.Login}' залогинился");
            return state;
        }  
        
        public async Task LogOutUser()
        {
            string userName = (await storage.GetAsync<string>("user_login")).Value ?? string.Empty;
            Log.Information($"Пользователь '{userName}' разлогинился");

            await storage.DeleteAsync("user_login");
            await storage.DeleteAsync("user_password");

            NotifyAuthenticationStateChanged(Task.FromResult(GetDefaultState()));            
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
