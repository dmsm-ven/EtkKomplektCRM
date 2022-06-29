using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services
{
    public interface IUserInfoChecker
    {
        Task FillUserInfo(AppUser user);
    }

    public class UserInfoChecker : IUserInfoChecker
    {
        private readonly IJSRuntime js;

        public UserInfoChecker(IJSRuntime js)
        {
            this.js = js;
        }

        public async Task FillUserInfo(AppUser user)
        {
            try
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>((await js.InvokeAsync<dynamic>("getUserInfo")).ToString());
                user.UserIP = userInfo["ipAddress"].ToString();
                user.UserCity = userInfo["city"].ToString();
            }
            catch
            {

            }
        }
    }
}
