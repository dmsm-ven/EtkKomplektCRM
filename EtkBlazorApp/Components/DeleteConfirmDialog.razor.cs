using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace EtkBlazorApp.Components
{
    public abstract class DeleteConfirmDialogBase : ComponentBase
    {
        protected bool ShowConfirmation { get; set; }

        [Parameter]
        public string ConfirmationTitle { get; set; } = "Заголовок";

        [Parameter]
        public Func<string> ConfirmationMessage { get; set; } = () => "Сообщение";

        public void Show()
        {
            ShowConfirmation = true;
            StateHasChanged();
        }

        [Parameter]
        public EventCallback<bool> ConfirmationChanged { get; set; }

        protected async Task OnConfirmationChange(bool value)
        {
            ShowConfirmation = false;
            await ConfirmationChanged.InvokeAsync(value);
        }
    }
}
