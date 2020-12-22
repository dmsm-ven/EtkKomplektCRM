using EtkBlazorApp.BL.Data;
using EtkBlazorApp.DataAccess;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MyCustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IDatabase db;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ProtectedLocalStorage storage;
        private readonly int PASSWORD_MAX_ENTER_TRY = 5;


        public MyCustomAuthProvider(IDatabase db, IHttpContextAccessor httpContextAccessor, ProtectedLocalStorage storage)
        {
            this.db = db;
            this.storage = storage;
            this.httpContextAccessor = httpContextAccessor;
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
            userData.IP = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            userData.InvalidPasswordCounter = await db.GetUserBadPasswordTryCounter(userData.IP);
            
            if(userData.InvalidPasswordCounter >= PASSWORD_MAX_ENTER_TRY)
            {
                return GetDefaultState();
            }

            string permission = await db.GetUserPermission(userData.Login, userData.Password);

            if (string.IsNullOrWhiteSpace(permission))
            {
                await db.SetUserBadPasswordTryCounter(userData.IP, userData.InvalidPasswordCounter + 1);
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
           
            await db.SetUserBadPasswordTryCounter(userData.IP, 0);
            return state;
        }  
        
        public async Task LogOutUser()
        {
            string userName = (await storage.GetAsync<string>("user_login")).Value ?? string.Empty;

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
