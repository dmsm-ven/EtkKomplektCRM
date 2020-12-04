using EtkBlazorApp.Data;
using EtkBlazorApp.DataAccess;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.SessionStorage;

namespace EtkBlazorApp.Services
{
    public class MyCustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IDatabase db;
        private readonly ISessionStorageService session;

        public MyCustomAuthProvider(IDatabase db, ISessionStorageService session)
        {
            this.db = db;
            this.session = session;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var login = await session.GetItemAsync<string>("user_login");
            var password = await session.GetItemAsync<string>("user_password");
            if (login != null && password != null)
            {
                var state = await AuthenticateUser(new UserModel() { Login = login, Password = password });
                return state;           
            }

            return GetDefaultState();            
        }

        public async Task<AuthenticationState> AuthenticateUser(UserModel userData)
        {
            if(!await db.GetUserPremission(userData.Login, userData.Password)) { return GetDefaultState(); }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userData.Login),
            }, "apiauth_type");

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return state;
        }  
        
        public void LogOutUser()
        {
            session.RemoveItemAsync("user_login");
            session.RemoveItemAsync("user_password");

            var state = GetDefaultState();


            NotifyAuthenticationStateChanged(Task.FromResult(state));
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
