using Dadata;
using Dadata.Model;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorAppi.Core.Data;

namespace EtkBlazorAppi.DadataApi;

public class DadataApiClient : ICompanyInfoChecker
{
    private readonly SuggestClientAsync api;

    public DadataApiClient(string token)
    {
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
            Capital = (data.capital?.value?.ToString() ?? "-"),
            Address = data.address.value,
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
            info.RegistrationDate = (data.state.registration_date?.ToString() ?? "-");
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
                Year = data.finance?.year?.ToString() ?? "-",
                Income = data.finance?.income?.ToString() ?? "-",
                Expense = data.finance?.expense?.ToString() ?? "-"
            };
        }

        return info;

    }
}


