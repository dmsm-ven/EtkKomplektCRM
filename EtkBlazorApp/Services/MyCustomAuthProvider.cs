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

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity(new[] 
            {
                new Claim(ClaimTypes.Name, "Гость")
            }, "");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));


            return Task.FromResult(new AuthenticationState(user));
        }

        public async Task AuthenticateUser(UserModel userData)
        {
            string role = await db.GetUserPremission(userData.Login, userData.Password);

            if (role != null)
            {
                var identity = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, userData.Login),
                new Claim(ClaimTypes.Role, role)
                }, "apiauth_type");

                var user = new ClaimsPrincipal(identity);
                var state = new AuthenticationState(user);

                NotifyAuthenticationStateChanged(Task.FromResult(state));
            }
        }       
    }
}
