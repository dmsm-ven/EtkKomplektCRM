using AutoMapper;
using Blazored.Toast.Services;
using EtkBlazorApp.BL;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.Settings.SettingsTabs
{
    public partial class UserSettingsTab
    {
        [Inject]
        public AuthenticationStateProvider stateProvider { get; set; }

        [Inject]
        public UserLogger logger { get; set; }

        [Inject]
        public IUserService userStorage { get; set; }

        [Inject]
        public IToastService toasts { get; set; }

        [Inject]
        public IMapper mapper { get; set; }

        [Parameter]
        public SettingsTabData tabData { get; set; }

        [CascadingParameter]
        public SettingsTabData selectedTab { get; set; }

        DeleteConfirmDialog deleteDialog;
        AddNewUserDialog addNewDialog;
        List<AppUserGroupEntity> groups;
        AppUser loginedUser = null;
        List<AppUser> users = null;
        public AppUser selectedUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            users = mapper.Map<List<AppUser>>(await userStorage.GetUsers());
            groups = await userStorage.GetUserGroups();
            string loginedUserName = (await stateProvider.GetAuthenticationStateAsync()).User.Identity.Name;
            loginedUser = users.Single(u => u.Login == loginedUserName);
        }

        private async Task UpdateUser(AppUser user)
        {
            await userStorage.UpdateUser(mapper.Map<AppUserEntity>(user));
            await logger.Write(LogEntryGroupName.Accounts, "Аккаунт обновлен", user.Login);
            user.Password = null;
            user = null;
            toasts.ShowSuccess("Данные пользователя обновлены");
        }

        private async Task DeleteConfirmed(bool dialogResult)
        {
            if (dialogResult)
            {
                await userStorage.DeleteUser(selectedUser.Id);
                toasts.ShowInfo($"Пользователь удален '{selectedUser.Login}'");
                await logger.Write(LogEntryGroupName.Accounts, "Аккаунт удален", selectedUser.Login);
                users.Remove(selectedUser);
                selectedUser = null;
            }
        }

        private async Task AddNewUserConfirmed(AppUser newUser)
        {
            if (newUser == null)
            {
                return;
            }

            await userStorage.AddUser(mapper.Map<AppUserEntity>(newUser));
            await logger.Write(LogEntryGroupName.Accounts, "Аккаунт добавлен", newUser.Login);
            toasts.ShowSuccess("Пользователь добавлен");
            users.Add(newUser);
            selectedUser = newUser;
        }
    }
}