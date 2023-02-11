using EtkBlazorApp.Core.Data;
using EtkBlazorApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtkBlazorApp;

//TODO: Возможно стоит заменить на фабрику
public class DeliveryServiceApiManager
{
    public IReadOnlyDictionary<TransportDeliveryCompany, ITransportCompanyApi> TkApiByName { get; }

    public DeliveryServiceApiManager(IEnumerable<ITransportCompanyApi> apis)
    {
        this.TkApiByName = apis
            .ToDictionary(i => i.Prefix, i => i);
    }

    public ITransportCompanyApi GetTkApiForOrder(string tk_order_id)
    {
        TransportDeliveryCompany tk = GetTkOrderPrefixByEnteredOrderNumber(tk_order_id);

        if (TkApiByName.TryGetValue(tk, out var api))
        {
            return api;
        }

        return null;
    }

    public TransportDeliveryCompany GetTkOrderPrefixByEnteredOrderNumber(string order_number)
    {
        //13 цифр, значит тут заказ от ТК Деловые линии
        if (Regex.IsMatch(order_number, @"^\d{13}$"))
        {
            return TransportDeliveryCompany.Dellin;
        }

        //10 цифр, значит тут заказ от ТК СДЭК
        if (Regex.IsMatch(order_number, @"^\d{10}$"))
        {
            return TransportDeliveryCompany.Cdek;
        }

        return TransportDeliveryCompany.None;
    }
}
