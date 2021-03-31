using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MyCustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IDatabaseAccess db;
        private readonly IAuthStateProcessor auth;
        private readonly MyDbLogger logger;
        private readonly ProtectedLocalStorage storage;

        public MyCustomAuthProvider(IAuthStateProcessor auth, MyDbLogger logger, ProtectedLocalStorage storage)
        {
            this.storage = storage;
            this.logger = logger;
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
                    Password = password.Value 
                });                
                return state;           
            }

            return GetDefaultState();            
        }

        public async Task<AuthenticationState> AuthenticateUser(AppUser userData)
        {           
            string permission = await auth.GetUserPermission(userData.Login, userData.Password);

            if (string.IsNullOrWhiteSpace(permission))
            {
                await logger.Write(LogEntryGroupName.Auth, userData.Login, "Неудача", $"Неудачная попытка входа {userData.Login}");
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

            await logger.Write(LogEntryGroupName.Auth, userData.Login, "Вход", $"Пользователь Вошел");
            await auth.UpdateUserLastLoginDate(userData.Login);

            return state;
        }  
        
        public async Task LogOutUser()
        {            
            string userName = (await storage.GetAsync<string>("user_login")).Value ?? string.Empty;
          
            await storage.DeleteAsync("user_login");
            await storage.DeleteAsync("user_password");

            await logger.Write(LogEntryGroupName.Auth, userName, "Выход", $"Пользователь вышел");

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
