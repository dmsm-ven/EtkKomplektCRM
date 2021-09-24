using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess.Entity;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages
{
    public partial class Stocks
    {
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
                var source = stockList.GroupBy(s => s.City ?? "Город не указан")
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
                stockList = (await manufacturerStorage.GetStocks())
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

        private void ShowStockField(StockPartnerViewModel stock, string propertyName)
        {
            string message = string.Empty;
            switch (propertyName)
            {
                case nameof(StockPartnerViewModel.Email): message = stock.Email; break;
                case nameof(StockPartnerViewModel.PhoneNumber): message = stock.PhoneNumber; break;
                case nameof(StockPartnerViewModel.Website): message = stock.Website; break;
            }
            toasts.ShowInfo(message, stock.Name);
        }

        private async Task NewStockDialogStatusChanged(StockPartnerViewModel data)
        {
            if (data != null)
            {
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
                    await manufacturerStorage.CreateOrUpdateStock(stockEntity);
                    data.Id = stockEntity.stock_partner_id;
                    stockList.Add(data);

                    await logger.Write(LogEntryGroupName.ManufacturerUpdate, "Добавление", $"Склад «{stockEntity.name}» добавлен");
                    toasts.ShowSuccess(data.Name, "Склад добавлен");
                }
                else
                {
                    await manufacturerStorage.CreateOrUpdateStock(stockEntity);
                    await logger.Write(LogEntryGroupName.ManufacturerUpdate, "Обновление", $"Данные склада «{stockEntity.name}» обновлены");
                    toasts.ShowSuccess(data.Name, "Склад обновлен");
                }
                StateHasChanged();
            }
        }

        private async void ShowStockAddress(StockPartnerViewModel stock)
        {
            toasts.ShowInfo(stock.Address ?? "Адрес не заполнен", stock.Name);
            await js.OpenAddressAsYandexMapLocation(stock.Address);
        }

        private async Task ShowLinkedStocksForBrand(ManufacturerViewModel manufacturer)
        {
            var data = await manufacturerStorage.GetManufacturerStockPartners(manufacturer.Id);

            if (data != null && data.Any())
            {
                string li = string.Join("\r\n", data.Select(i => $"<li>{i.name} - {i.total_products} товаров ({i.total_quantity} шт.)</li>"));
                RenderFragment messageFragment() => builder =>
                {
                    builder.AddContent(1, new MarkupString($"<ul>{li}</ul>"));
                };

                toasts.ShowInfo(messageFragment(), manufacturer.name);
            }
            else
            {
                toasts.ShowInfo("Нет данных по количеству товаров для данного производителя", manufacturer.name);
            }
        }

        private async Task ShowStockLinkedManufacturers(StockPartnerViewModel stock)
        {
            var data = await manufacturerStorage.GetStockManufacturers(stock.Id);

            if (data != null && data.Any())
            {
                string li = string.Join("\r\n", data.Select(i => $"<li>{i.name} - {i.total_products} товаров ({i.total_quantity} шт.)</li>"));
                RenderFragment messageFragment() => builder =>
                {
                    builder.AddContent(1, new MarkupString($"<ul>{li}</ul>"));
                };

                toasts.ShowInfo(messageFragment(), stock.Name);
            }
            else
            {
                toasts.ShowInfo("Нет данных по брендам для данного склада", stock.Name);
            }
        }

    }
}
