using EtkBlazorApp.DataAccess;
using EtkBlazorApp.Extensions;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services;

public class CashPlusPlusLinkGenerator
{
    private readonly ISettingStorage settingStorage;

    public CashPlusPlusLinkGenerator(ISettingStorage settingStorage)
    {
        this.settingStorage = settingStorage;
    }

    public async Task<string> GenerateLink(string order_id)
    {
        if (string.IsNullOrWhiteSpace(order_id))
        {
            return string.Empty;
        }

        var shop_config_encyption_phrase = await settingStorage.GetValue("shop_config_encyption_phrase");

        string code = StringHelper.GetMD5(order_id + shop_config_encyption_phrase)
            .Substring(0, 12)
            .ToLower();

        string pdfUri = $"https://etk-komplekt.ru/index.php?route=account/cash_plusplus/view&code={code}&order={order_id}";
        return pdfUri;
    }
}
