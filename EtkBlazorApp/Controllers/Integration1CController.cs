using EtkBlazorApp.Core.Data.Integration1C;
using EtkBlazorApp.Model.IOptionProfiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Controllers;

[ApiController]
[Route("api/integration-1c")]
public class Integration1CController : Controller
{
    private readonly Integration1C_Configuration options;

    public Integration1CController(IOptions<Integration1C_Configuration> options)
    {
        this.options = options.Value;
    }

    [HttpPost("update-stock")]
    public IActionResult StockUpdated([FromBody] StoreStockData[] newData)
    {
        if (!HttpContext.Request.Headers.TryGetValue("Authorize", out var auth))
        {
            return BadRequest($"Не указан токен интеграции");
        }

        string senderToken = auth.ToString()
            .Replace("Token", string.Empty)
            .Trim();

        bool isAuthorized = senderToken.Equals(options.Token, System.StringComparison.OrdinalIgnoreCase);

        if (!isAuthorized)
        {
            return Unauthorized($"Токен указан не верно: {senderToken}");
        }

        decimal totalQuantity = newData[0]
            .Sku
            .Select(sku => decimal.Parse(sku.quantity))
            .Sum();

        return Ok($"Сумма остатков по всем артикулам: {totalQuantity}");
    }
}

