using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages
{
    public partial class Partners
    {
        List<PartnerViewModel> partners = null;
        List<ManufacturerViewModel> manufacturers = null;
        PartnerViewModel selectedPartner = null;
        ConfirmDialog deleteDialog;
        ConfirmDialog changePasswordDialog;

        Timer lastAccessRefreshTimer;

        bool isNewPartner => selectedPartner.Id == Guid.Empty;
        bool blinkLastAccessLabel = false;

        protected override void OnInitialized()
        {
            lastAccessRefreshTimer = new Timer(UpdateLastAccessLabel, null, 0, 5000);
        }

        private async void UpdateLastAccessLabel(object timer)
        {
            string guid = selectedPartner?.Id.ToString();
            if(string.IsNullOrWhiteSpace(guid) || guid == Guid.Empty.ToString()) { return; }

            var lastAccess = await partnerService.GetPartnerLastAccessDateTime(guid);
            if(selectedPartner.PriceListLastAccessDateTime != lastAccess)
            {
                blinkLastAccessLabel = true;
                selectedPartner.PriceListLastAccessDateTime = lastAccess;
                await InvokeAsync(StateHasChanged);
                blinkLastAccessLabel = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) { return; }

            partners = (await partnerService.GetAllPartners())
                .Select(p => new PartnerViewModel()
                {
                    Id = Guid.Parse(p.id),
                    Address = p.address,
                    ContactPerson = p.contact_person,
                    Created = p.created,
                    Description = p.description,
                    Discount = p.discount,
                    Email = p.email,
                    Name = p.name,
                    Password = p.price_list_password,
                    PhoneNumber = p.phone_number,
                    PriceListLastAccessDateTime = p.price_list_last_access,
                    Priority = p.priority,
                    Updated = p.updated,
                    Website = p.website
                })                
                .OrderByDescending(p => p.Priority)
                .ThenByDescending(p => p.Discount)
                .ToList();

            manufacturers = (await manufacturerStorage.GetManufacturers())
                .Select(m => new ManufacturerViewModel() { name = m.name, Id = m.manufacturer_id })
                .ToList();

            if (Guid.TryParse(await settingsStorage.GetValue("last_viewed_partner_id"), out var lastViewerPartnerId))
            {
                await OnPartnerChanged(partners.FirstOrDefault(p => p.Id == lastViewerPartnerId));
            }

            StateHasChanged();

        }

        private async Task OnPartnerChanged(PartnerViewModel newPartner)
        {        
            if(newPartner?.Id.ToString() == null) { return; }

            await settingsStorage.SetValue("last_viewed_partner_id", newPartner.Id.ToString());

            newPartner.CheckedManufacturers = (await partnerService.GetPartnerManufacturers(newPartner.Id.ToString()))
                .Select(m => new ManufacturerViewModel()
                {
                    Id = m.manufacturer_id,
                    name = m.name
                })
                .ToList();
            selectedPartner = newPartner;
        }

        private Dictionary<ManufacturerViewModel, bool> OrderedManufacturersForSelectedPartner
        {
            get
            {
                var data = manufacturers.Select(m => new
                {
                    Manufacturer = m,
                    IsChecked = selectedPartner.CheckedManufacturers?.FirstOrDefault(ma => ma.Id == m.Id) != null
                })
                .OrderBy(d => d.IsChecked ? 0 : 1)
                .ToDictionary(i => i.Manufacturer, j => j.IsChecked);

                return data;
            }
        }

        private async Task OnManufacturerButtonClick(ManufacturerViewModel manufacturer, bool isChecked)
        {
            if (isChecked)
            {
                var item = selectedPartner.CheckedManufacturers.FirstOrDefault(m => m.Id == manufacturer.Id);
                selectedPartner.CheckedManufacturers.Remove(item);
                await partnerService.RemoveManufacturerFromPartner(selectedPartner.Id.ToString(), manufacturer.Id);
            }
            else
            {
                selectedPartner.CheckedManufacturers.Add(manufacturer);
                await partnerService.AddManufacturerToPartner(selectedPartner.Id.ToString(), manufacturer.Id);
            }
            StateHasChanged();
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
                toasts.ShowInfo(selectedPartner.Name, "Партнер удален");
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
            selectedPartner = new PartnerViewModel() { 
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
                toasts.ShowSuccess(selectedPartner.Name, "Партнер добавлен");
                selectedPartner.Id = Guid.Parse(entity.id);
            }
            else
            {
                await logger.Write(LogEntryGroupName.Partners, "Обновление", $"Информация о '{selectedPartner.Name}' обновлена");
                toasts.ShowSuccess(selectedPartner.Name, "Обновлено");
            }
            selectedPartner.Updated = dt;
        }
    
        public void Dispose()
        {
            lastAccessRefreshTimer?.Dispose();
        }
    }
}
