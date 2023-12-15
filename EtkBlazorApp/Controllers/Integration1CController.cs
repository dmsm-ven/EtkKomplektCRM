using EtkBlazorApp.Core.Data.Integration1C;
using EtkBlazorApp.Model.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EtkBlazorApp.Controllers;

[ApiController]
[Integration1CAuthorize]
[Route("api/integration-1c")]
public class Integration1CController : Controller
{
    [HttpPost("update-stock")]
    public IActionResult StockUpdated([FromBody] StoreStockData[] newData)
    {
        decimal totalQuantity = newData[0]
            .Sku
            .Select(sku => decimal.Parse(sku.quantity))
            .Sum();

        return Ok($"Сумма остатков по всем артикулам: {totalQuantity}");
    }
}
