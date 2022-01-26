using EtkBlazorApp.DataAccess;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using EtkBlazorApp.DataAccess.Entity;

namespace EtkBlazorApp.Services
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IAuthenticationDataStorage auth;
        private readonly IUserInfoChecker userInfoChecker;
        private readonly IJSRuntime js;
        private readonly ProtectedLocalStorage storage;

        //HMACSHA256 + salt
        //https://docs.microsoft.com/ru-ru/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-6.0
        public CustomAuthProvider(
            IAuthenticationDataStorage auth,
            IUserInfoChecker userInfoChecker,
            IJSRuntime js,
            ProtectedLocalStorage storage)
        {
            this.storage = storage;
            this.auth = auth;
            this.userInfoChecker = userInfoChecker;
            this.js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
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
            }
            catch
            {

            }

            return GetDefaultState();            
        }

        public async Task<AuthenticationState> AuthenticateUser(AppUser userData)
        {
            await userInfoChecker.FillUserInfo(userData);

            var userInfo = await auth.GetUser(userData.Login, userData.Password);

            //Нет доступа (не верный логин и/или пароль)
            if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.permission))
            {
                return GetDefaultState();
            }

            //Проверяем, есть ли привязка к IP 
            if (!string.IsNullOrWhiteSpace(userInfo?.ip) && (userInfo?.ip != userData?.UserIP))
            {
                return GetDefaultState();
            }

            //Создаем claims и State
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userInfo.login),
                new Claim(ClaimTypes.Role, userInfo.permission)
            }, "login_form");
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));
            await auth.UpdateUserLastLoginDate(userInfo.login);
            SetUserCookie(userInfo.login);

            return state;
        }
       
        private void SetUserCookie(string name)
        {
            //cookie для того, если user активен в ЛК то отображать склады на странице товара
            string cookieString = $"lk_user={name}; expires=9999-12-31T23:59:59.000Z;path=/;domain=etk-komplekt.ru";
            js.InvokeVoidAsync("CookieFunction.acceptMessage", cookieString);
        }

        public async Task LogOutUser()
        {
            try
            {
                await storage.DeleteAsync("user_login");
                await storage.DeleteAsync("user_password");
            }
            catch
            {

            }
            NotifyAuthenticationStateChanged(Task.FromResult(GetDefaultState()));           
        }

        private AuthenticationState GetDefaultState()
        {
            storage.DeleteAsync("user_login");
            storage.DeleteAsync("user_password");

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
