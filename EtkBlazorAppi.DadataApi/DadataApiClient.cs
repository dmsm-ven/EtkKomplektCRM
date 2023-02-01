using Dadata;
using Dadata.Model;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorAppi.Core.Data;
using EtkBlazorAppi.Core.Data.CompanyInfo;
using System.Net.Http.Json;
using System.Text.Json;

namespace EtkBlazorAppi.DadataApi;

public class DadataApiClient : ICompanyInfoChecker
{
    private readonly SuggestClientAsync api;
    private readonly string token;

    public DadataApiClient(string token)
    {
        this.token = token;
        api = new SuggestClientAsync(token);
    }

    public async Task<CompanyInformation?> GetInfoByInn(string inn)
    {
        SuggestResponse<Party> apiResponse = null;

        try
        {
            apiResponse = await api.FindParty(inn);
        }
        catch
        {
            return null;
        }

        var result = apiResponse?.suggestions?.FirstOrDefault();
        var data = result?.data;

        if (result == null || data == null) { return null; }

        var info = new CompanyInformation()
        {
            CompanyName = result.value,
            Capital = data.capital?.value,
            Address = data.address.value,
            AddressUnrestricted = data.address.unrestricted_value,
            ActualityDate = result.data?.state?.actuality_date?.ToString()?.Replace(" 0:00:00", string.Empty) ?? "-",
            Codes = new CompanyGeneralCodes()
            {
                Kpp = data.kpp,
                Inn = data.inn,
                Ogrn = data.ogrn,
                Okpo = data.okpo,
                Okato = data.okato,
                Oktmo = data.oktmo,
                Okogu = data.okogu,
                Okfs = data.okfs,
                Okved = data.okved
            }
        };

        if (data.state != null)
        {
            info.Status = data.state.status.ToString();
            info.RegistrationDate = (data.state.registration_date?.ToString().Replace(" 0:00:00", string.Empty) ?? "-");
        }

        if (data.management != null)
        {
            info.Managment = new CompanyManagmentInformation()
            {
                Name = data.management.name,
                Post = data.management.post,
                Disqualified = data.management.disqualified
            };
        }

        if (data.finance != null)
        {
            info.FinanceInfo = new CompanyFinanceInformation()
            {
                Year = data.finance?.year,
                Income = data.finance?.income,
                Expense = data.finance?.expense,
                Debt = data.finance?.debt
            };
        }

        return info;
    }

    private async Task<Party> GetInfoByInnCustom(string inn)
    {
        var rawClient = new HttpClient();
        rawClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        rawClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Token {token}");
        rawClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", $"application/json");

        var rawContent = JsonContent.Create(new { query = inn });
        var rawResult = await rawClient.PostAsync("https://suggestions.dadata.ru/suggestions/api/4_1/rs/findById/party", rawContent);
        if (rawResult.IsSuccessStatusCode)
        {
            var rawResponse = await rawResult.Content.ReadAsStringAsync();
            var rawResponseObj = JsonSerializer.Deserialize<Party>(rawResponse);
            return rawResponseObj;
        }

        return null;
    }
}


