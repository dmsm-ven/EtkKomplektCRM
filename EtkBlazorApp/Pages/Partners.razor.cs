using AutoMapper;
using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages
{
    //TODO: разбиться класс на более простые блоки, стоит подключить Mapper вместо ручного создания сущ.
    public partial class Partners
    {
        [Inject] public IToastService toasts { get; set; }
        [Inject] public IJSRuntime js { get; set; }
        [Inject] public IManufacturerStorage manufacturerStorage { get; set; }
        [Inject] public ISettingStorageReader settingsStorageReader { get; set; }
        [Inject] public ISettingStorageWriter settingsStorageWriter { get; set; }
        [Inject] public IPartnersInformationService partnerService { get; set; }
        [Inject] public IMapper mapper { get; set; }
        [Inject] public UserLogger logger { get; set; }

        ConfirmDialog deleteDialog;
        ConfirmDialog changePasswordDialog;
        CustomDataDialog historyRequestDialog;
        Timer lastAccessRefreshTimer;

        bool blinkLastAccessLabel = false;
        List<PartnerViewModel> partners = null;
        List<ManufacturerViewModel> manufacturers = null;
        PartnerViewModel selectedPartner = null;
        PartnerManufacturerDiscountItemViewModel editingDiscount = null;

        bool isNewPartner => selectedPartner.Id == Guid.Empty;

        protected override void OnInitialized()
        {
            lastAccessRefreshTimer = new Timer(UpdateLastAccessLabel, null, 0, 5000);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) { return; }

            partners = mapper.Map<IEnumerable<PartnerViewModel>>(await partnerService.GetAllPartners())
                .OrderByDescending(p => p.Priority)
                .ThenByDescending(p => p.Discount)
                .ToList();

            manufacturers = (await manufacturerStorage.GetManufacturers())
                .Select(m => new ManufacturerViewModel() { name = m.name, Id = m.manufacturer_id })
                .ToList();

            if (Guid.TryParse(await settingsStorageReader.GetValue("last_viewed_partner_id"), out var lastViewerPartnerId))
            {
                await OnPartnerChanged(partners.FirstOrDefault(p => p.Id == lastViewerPartnerId));
            }

            StateHasChanged();
        }

        private async Task OnPartnerChanged(PartnerViewModel newPartner)
        {
            if (newPartner?.Id.ToString() == null) { return; }

            await settingsStorageWriter.SetValue("last_viewed_partner_id", newPartner.Id.ToString());

            newPartner.DiscountBrandsInfo = (await partnerService.GetPartnerManufacturers(newPartner.Id.ToString()))
                .Select(m => new PartnerManufacturerDiscountItemViewModel()
                {
                    PartnerGuid = Guid.Parse(m.partner_id),
                    ManufacturerId = m.manufacturer_id,
                    ManufacturerName = m.name,
                    Discount = m.discount
                })
                .ToList();
            selectedPartner = newPartner;
        }

        private List<PartnerManufacturerDiscountItemViewModel> ManufacturerButtonsSource
        {
            get
            {
                var data = manufacturers
                    .Select(m => new PartnerManufacturerDiscountItemViewModel()
                    {
                        PartnerGuid = selectedPartner.Id,
                        Discount = selectedPartner.DiscountBrandsInfo.FirstOrDefault(b => b.ManufacturerId == m.Id)?.Discount,
                        ManufacturerId = m.Id,
                        ManufacturerName = m.name
                    })
                    .OrderBy(d => selectedPartner.DiscountBrandsInfo.FirstOrDefault(b => b.ManufacturerId == d.ManufacturerId) == null ? 1 : 0)
                    .ToList();

                return data;
            }
        }

        private async Task RemoveBrandDiscount(PartnerManufacturerDiscountItemViewModel item)
        {
            selectedPartner.RemoveItem(item);
            await partnerService.RemoveManufacturerFromPartner(item.PartnerGuid.ToString(), item.ManufacturerId);
            StateHasChanged();
        }

        private async Task AddAndSaveBrandDiscount(PartnerManufacturerDiscountItemViewModel item)
        {
            if (!selectedPartner.HasItem(item))
            {
                selectedPartner.DiscountBrandsInfo.Add(item);
            }

            if (item?.Discount != null && item.Discount.Value == selectedPartner.Discount)
            {
                item.Discount = null;
            }

            await partnerService.AddManufacturerToPartner(item.PartnerGuid.ToString(), item.ManufacturerId, item?.Discount);
            editingDiscount = null;
        }

        private async Task GeneratePassword()
        {
            selectedPartner.Password = await PasswordGenerator.GeneratePassword(length: 8);
            StateHasChanged();
        }

        private async Task ConfirmStatusChanged(bool status)
        {
            if (status)
            {
                partners.Remove(selectedPartner);
                await partnerService.DeletePartner(selectedPartner.Id.ToString());
                await logger.Write(LogEntryGroupName.Partners, "Удаление", $"Партнер '{selectedPartner.Name}' удален из системы");
                toasts.ShowInfo($"Партнер удален: {selectedPartner.Name}");
                selectedPartner = null;
            }
        }

        private async Task PasswordDialogStatusChanged(bool status)
        {
            if (status)
            {
                await GeneratePassword();
            }
        }

        private async Task AddNewPartner()
        {
            selectedPartner = new PartnerViewModel()
            {
                Priority = 1,
                Name = "Новый партнер",
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Updated = DateTime.Now
            };
            partners.Insert(0, selectedPartner);
            await GeneratePassword();
            StateHasChanged();
        }

        private async Task OnPartnerChangeSubmitted()
        {
            var dt = DateTime.Now;

            var entity = new PartnerEntity()
            {
                id = isNewPartner ? Guid.NewGuid().ToString() : selectedPartner.Id.ToString(),
                address = selectedPartner.Address,
                contact_person = selectedPartner.ContactPerson,
                discount = selectedPartner.Discount,
                email = selectedPartner.Email,
                name = selectedPartner.Name,
                price_list_password = selectedPartner.Password,
                phone_number = selectedPartner.PhoneNumber,
                priority = selectedPartner.Priority,
                updated = dt,
                created = selectedPartner.Created,
                website = selectedPartner.Website,
                description = selectedPartner.Description
            };

            await partnerService.AddOrUpdatePartner(entity);

            if (isNewPartner)
            {
                await logger.Write(LogEntryGroupName.Partners, "Добавление", $"Партнер '{selectedPartner.Name}' добавлен");
                toasts.ShowSuccess($"Партнер добавлен: {selectedPartner.Name}");
                selectedPartner.Id = Guid.Parse(entity.id);
            }
            else
            {
                await logger.Write(LogEntryGroupName.Partners, "Обновление", $"Информация о '{selectedPartner.Name}' обновлена");
                toasts.ShowSuccess($"Обновлено: {selectedPartner.Name}");
            }
            selectedPartner.Updated = dt;
        }

        private void BrandDiscountLabelClicked(PartnerManufacturerDiscountItemViewModel item)
        {
            if (editingDiscount?.ManufacturerId == item.ManufacturerId)
            {
                editingDiscount = null;
            }
            else
            {
                editingDiscount = selectedPartner.DiscountBrandsInfo.FirstOrDefault(i => i.ManufacturerId == item.ManufacturerId);
                editingDiscount.Discount = item.Discount.HasValue ? (int)item.Discount : (int)selectedPartner.Discount;
            }
            StateHasChanged();
        }

        private void ChangeSelectedBrandDiscount(int direction)
        {
            editingDiscount.Discount += direction;
            StateHasChanged();
        }

        private async void UpdateLastAccessLabel(object timer)
        {
            string guid = selectedPartner?.Id.ToString();
            if (string.IsNullOrWhiteSpace(guid) || guid == Guid.Empty.ToString()) { return; }

            var lastAccess = await partnerService.GetPartnerLastAccessDateTime(guid);
            if (selectedPartner.PriceListLastAccessDateTime != lastAccess)
            {
                blinkLastAccessLabel = true;
                selectedPartner.PriceListLastAccessDateTime = lastAccess;
                await InvokeAsync(StateHasChanged);
                blinkLastAccessLabel = false;
            }
        }

        private async Task ShowHistoryDialog()
        {
            selectedPartner.RequestHistory = await partnerService.GetPartnerRequestHistory(selectedPartner.Id.ToString(), limit: 100);
            historyRequestDialog.Show();
        }

        public void Dispose()
        {
            lastAccessRefreshTimer?.Dispose();
        }
    }
}
