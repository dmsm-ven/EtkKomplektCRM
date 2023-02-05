using AutoMapper;
using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.Settings.SettingsTabs
{
    public partial class MonobrandSettingsTab
    {
        [CascadingParameter]
        public SettingsTabData tabData { get; set; }

        [Inject]
        public IManufacturerStorage manufacturerStorage { get; set; }

        [Inject]
        public IMonobrandStorage monobrandStorage { get; set; }

        [Inject]
        public IMapper mapper { get; set; }

        [Inject]
        public ISettingStorage settings { get; set; }

        [Inject]
        public NavigationManager navigationManager { get; set; }

        [Inject]
        public UserLogger logger { get; set; }

        DeleteConfirmDialog confirmDialog;
        List<MonobrandViewModel> monobrands = null;
        MonobrandViewModel selectedMonobrand = null;
        List<ManufacturerEntity> manufacturers = null;
        List<string> currencyCodeList = new List<string>(Enum.GetNames(typeof(CurrencyType)));

        bool isMonobrandUpdateEnabled;
        string monobrandKey;

        protected override async Task OnInitializedAsync()
        {
            await RefreshMonobrandList();

            manufacturers = await manufacturerStorage.GetManufacturers();
            isMonobrandUpdateEnabled = await settings.GetValue<bool>("update-monobrand-websites");
            monobrandKey = await settings.GetValue("monobrand_updater_key");

            tabData.SaveButtonClicked = new Action(async () =>
            {
                await settings.SetValue("update-monobrand-websites", isMonobrandUpdateEnabled);
                await settings.SetValue("monobrand_updater_key", monobrandKey);
            });
        }

        private async Task MonobrandStatusChanged(ChangeEventArgs e)
        {
            selectedMonobrand.IsUpdateEnabled = !selectedMonobrand.IsUpdateEnabled;
            await SaveChanges();
        }

        private async Task ManufacturerChanged(ChangeEventArgs e)
        {
            var brandInfo = manufacturers.Single(m => m.name.Equals((string)e.Value));
            selectedMonobrand.ManufacturerName = brandInfo.name;
            selectedMonobrand.ManufacturerId = brandInfo.manufacturer_id;
            await SaveChanges();
        }

        private async Task CurrencyCodeChanged(ChangeEventArgs e)
        {
            selectedMonobrand.CurrencyCode = (string)e.Value;
            await SaveChanges();
        }

        private async Task AddMonobrand()
        {
            await monobrandStorage.AddMonobrand();
            await RefreshMonobrandList();
        }

        private async Task DialogStatusChanged(bool status)
        {
            if (status)
            {
                await monobrandStorage.DeleteMonobrand(selectedMonobrand.MonobrandId);
                await logger.Write(LogEntryGroupName.ManufacturerUpdate, "Монобренд удален", $"Из списка удален монобренд: '{selectedMonobrand.WebsiteUri}'");
                monobrands.Remove(selectedMonobrand);
                selectedMonobrand = null;
            }
        }

        private async Task SaveChanges()
        {
            if (selectedMonobrand == null)
            {
                return;
            }

            await monobrandStorage.UpdateMonobrand(mapper.Map<MonobrandEntity>(selectedMonobrand));
        }

        private async Task RefreshMonobrandList()
        {
            monobrands = mapper.Map<List<MonobrandViewModel>>(await monobrandStorage.GetMonobrands());
        }
    }
}