using Blazored.Toast.Services;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Repositories.Product;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages;

public partial class Stocks
{
    [Inject] public IManufacturerStorage manufacturerStorage { get; set; }
    [Inject] public IStockStorage stockStorage { get; set; }
    [Inject] public IToastService toasts { get; set; }
    [Inject] public IJSRuntime js { get; set; }
    [Inject] public UserLogger logger { get; set; }

    Dictionary<char, List<ManufacturerViewModel>> manufacturersByFirstLetter = null;
    List<StockPartnerViewModel> stockList = null;
    List<StockPartnerViewModel> filteredStockList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(cityFilter))
            {
                return stockList;
            }
            return stockList.Where(s => s.City == cityFilter).ToList();
        }
    }

    AddNewStockDialog newStockDialog;
    string cityFilter = string.Empty;
    Dictionary<string, int> stockCities
    {
        get
        {
            var source = stockList
                .Where(i => i.CityId != 0)
                .GroupBy(s => s.City ?? "Город не указан")
                .OrderByDescending(i => i.Count())
                .ThenBy(i => i.Key)
                .ToDictionary(i => i.Key, j => j.Count());

            return source;
        }
    }
    bool hasAlphabetSeparator = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            stockList = (await stockStorage.GetStocks())
                .Select(model => new StockPartnerViewModel()
                {
                    Id = model.stock_partner_id,
                    ShipmentPeriodInDays = model.shipment_period,
                    Name = model.name,
                    Description = model.description,
                    City = model.city,
                    CityId = model.city_id,
                    Address = model.address,
                    PhoneNumber = model.phone_number,
                    Email = model.email,
                    ShowNameForAll = model.show_name_for_all,
                    Website = model.website
                })
                .ToList();

            manufacturersByFirstLetter = (await manufacturerStorage.GetManufacturers())
            .Select(model => new ManufacturerViewModel()
            {
                Id = model.manufacturer_id,
                OldShipmentPeriod = model.shipment_period,
                ShipmentPeriodInDays = model.shipment_period,
                name = model.name,
                keyword = model.keyword,
                productsCount = model.productsCount
            })
            .GroupBy(m => m.name[0])
            .ToDictionary(i => i.Key, j => j.OrderBy(m => m.name).ToList());

            StateHasChanged();
        }
    }

    private async Task NewStockDialogStatusChanged(StockPartnerViewModel data)
    {
        if (data == null) { return; }

        var stockEntity = new StockPartnerEntity()
        {
            stock_partner_id = data.Id,
            shipment_period = data.ShipmentPeriodInDays,
            city = data.City,
            city_id = data.CityId,
            description = data.Description,
            name = data.Name,
            phone_number = data.PhoneNumber,
            address = data.Address,
            email = data.Email,
            show_name_for_all = data.ShowNameForAll,
            website = data.Website
        };

        if (data.Id == 0)
        {
            await stockStorage.CreateOrUpdateStock(stockEntity);
            data.Id = stockEntity.stock_partner_id;
            stockList.Add(data);

            await logger.Write(LogEntryGroupName.ManufacturerUpdate, "Добавление", $"Склад «{stockEntity.name}» добавлен");
            toasts.ShowSuccess($"Склад добавлен: {data.Name}");
        }
        else
        {
            await stockStorage.CreateOrUpdateStock(stockEntity);
            await logger.Write(LogEntryGroupName.ManufacturerUpdate, "Обновление", $"Данные склада «{stockEntity.name}» обновлены");
            toasts.ShowSuccess($"Склад обновлен: {data.Name}");
        }
        StateHasChanged();
    }

    private async void ShowStockAddress(StockPartnerViewModel stock)
    {
        toasts.ShowInfo($"{stock.Name}: {stock.Address ?? "Адрес не заполнен"}");
        await js.OpenAddressAsYandexMapLocation(stock.Address);
    }

    private async Task ShowLinkedStocksForBrand(ManufacturerViewModel manufacturer)
    {
        var data = await stockStorage.GetManufacturerStockPartners(manufacturer.Id);

        if (data != null && data.Any())
        {
            string li = string.Join("\r\n", data.Select(i => $"<li>{i.name} - {i.total_products} товаров ({i.total_quantity} шт.)</li>"));

            RenderFragment messageFragment() => builder => builder.AddContent(1, new MarkupString($"<p>{manufacturer.name}</p><ul>{li}</ul>"));

            toasts.ShowInfo(messageFragment());
        }
        else
        {
            toasts.ShowInfo($"{manufacturer.name}: Нет данных по количеству товаров");
        }
    }

    private async Task ShowStockLinkedManufacturers(StockPartnerViewModel stock)
    {
        var data = await stockStorage.GetStockManufacturers(stock.Id);

        if (data != null && data.Any())
        {
            string li = string.Join("\r\n", data.Select(i => $"<li>{i.name} - {i.total_products} товаров ({i.total_quantity} шт.)</li>"));

            RenderFragment messageFragment() => builder => builder.AddContent(1, new MarkupString($"<p>{stock.Name}</p><ul>{li}</ul>"));

            toasts.ShowInfo(messageFragment());
        }
        else
        {
            toasts.ShowInfo($"{stock.Name}: Нет данных по брендам для данного склада");
        }
    }

}

