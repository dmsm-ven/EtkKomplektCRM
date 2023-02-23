using EtkBlazorApp.DataAccess;
using EtkBlazorApp.Extensions;
using System.Threading.Tasks;

namespace EtkBlazorApp.Services;

public class CashPlusPlusLinkGenerator
{
    private readonly ISettingStorageReader settingStorage;

    public CashPlusPlusLinkGenerator(ISettingStorageReader settingStorage)
    {
        this.settingStorage = settingStorage;
    }

    public async Task<string> GenerateLink(int order_id)
    {
        if (order_id == 0)
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
