using EtkBlazorApp.DataAccess;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public class MyCustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IAuthenticationDataStorage auth;
        private readonly IUserInfoChecker userInfoChecker;
        private readonly ProtectedLocalStorage storage;

        public MyCustomAuthProvider(
            IAuthenticationDataStorage auth,
            IUserInfoChecker userInfoChecker,
            ProtectedLocalStorage storage)
        {
            this.storage = storage;
            this.auth = auth;
            this.userInfoChecker = userInfoChecker;
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
            await userInfoChecker.FillUserInfo(userData);

            string permission = await auth.GetUserPermission(userData.Login, userData.Password);

            if (string.IsNullOrWhiteSpace(permission))
            {
                return GetDefaultState();
            }

            var userInfo = (await auth.GetUsers()).Single(u => u.login == userData.Login);
       

            if (!string.IsNullOrWhiteSpace(userInfo.ip) && userInfo.ip != userData.UserIP)
            {
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

            return state;
        }  
        
        public async Task LogOutUser()
        {                      
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
