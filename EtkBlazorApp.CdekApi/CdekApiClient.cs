using EtkBlazorApp.Core.Interfaces;

namespace EtkBlazorApp.CdekApi;

public class CdekApiClient : ITransportCompanyApi
{
    private readonly string account;
    private readonly string securePassword;
    private readonly HttpClient client;

    private string jwtToken;

    private bool IsValidToken => JwtUtils.ValidateToken(jwtToken, securePassword);
    

    public CdekApiClient(string account, string securePassword)
    {
        this.account = account;
        this.securePassword = securePassword;

        client = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            CookieContainer =new System.Net.CookieContainer(),
        });
        client.BaseAddress = new Uri("https://api.cdek.ru/v2");
    }

    private async Task AuthorizeIfNeed()
    {
        if (!IsValidToken)
        {

        }
    }

    public async Task<dynamic> GetOrderInfo(string cdekOrderNumber)
    {
        await AuthorizeIfNeed();
    }
}
