using AutoMapper;
using Blazored.Toast.Services;
using EtkBlazorApp.BL.Data;
using EtkBlazorApp.Components.Dialogs;
using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace EtkBlazorApp.Pages.Order
{
    public partial class OrderDetails
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public IOrderStorage OrderStorage { get; set; }

        [Inject]
        public IOrderUpdateService OrderUpdateService { get; set; }

        [Inject]
        public IToastService Toasts { get; set; }

        [Inject]
        public IJSRuntime Js { get; set; }

        [Inject]
        public IMapper Mapper { get; set; }

        [Inject]
        public ICompanyInfoChecker CompanyInfoChecker { get; set; }

        [Inject]
        public CashPlusPlusLinkGenerator OrderPdfLinkGenerator { get; set; }

        [Inject]
        public DeliveryServiceApiManager DeliveryManager { get; set; }

        [Inject]
        public NavigationManager NavManager { get; set; }

        [Inject]
        public UserLogger Logger { get; set; }

        EtkBlazorApp.Order order = null;
        CustomDataDialog statusChangesDialog;

        protected override async Task OnInitializedAsync()
        {
            var orderData = await OrderStorage.GetOrderById(Id);
            order = Mapper.Map<EtkBlazorApp.Order>(orderData);
        }

        private async Task GoToPrintPage()
        {
            //������ �� �������� � ������ ��������
            //string printPage = NavManager.Uri + "/print";
            //������ �� �������� �� ������, ��� ������������ � ������ cash_plusplus
            string printPage = await OrderPdfLinkGenerator.GenerateLink(order.OrderId);
            await Js.InvokeAsync<object>("open", printPage, "_blank");
        }

        private async Task ChangeTkOrderNumber(string newCode)
        {
            string oldNumber = order.TkOrderNumber;
            if (oldNumber != newCode)
            {
                var tk = DeliveryManager.GetTkOrderPrefixByEnteredOrderNumber(newCode);
                if (tk != TransportDeliveryCompany.None)
                {
                    await OrderUpdateService.ChangeOrderLinkedTkNumber(order.OrderId, newCode, tk);
                    Toasts.ShowInfo($"������������ ����� ������ � �� ��������");
                    await Logger.Write(LogEntryGroupName.Orders, "� �� �������", $"������� ������������ ����� ������ �� '{tk}' �� '{newCode}' ��� ������ {order.OrderId}");
                }
                else
                {
                    Toasts.ShowError($"�� ������� ������������� � �� ��� ������ � ����� �������");
                }
            }
        }

        private void ShowStatusChangesDialog()
        {
            statusChangesDialog.Show();
        }
    }
}