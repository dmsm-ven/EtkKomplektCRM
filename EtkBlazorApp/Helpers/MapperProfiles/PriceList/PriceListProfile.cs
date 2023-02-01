using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;
using System.Linq;

namespace EtkBlazorApp.Helpers.MapperProfiles.PriceList;

public class PriceListProfile : Profile
{
    public PriceListProfile()
    {
        CreateMap<PriceListTemplateEntity, PriceListTemplateItemViewModel>()
            .ForMember(o => o.Guid, o => o.MapFrom(x => x.id))
            .ForMember(o => o.Title, o => o.MapFrom(x => x.title))
            .ForMember(o => o.Description, o => o.MapFrom(x => x.description))
            .ForMember(o => o.Discount, o => o.MapFrom(x => x.discount))
            .ForMember(o => o.GroupName, o => o.MapFrom(x => x.group_name))
            .ForMember(o => o.Image, o => o.MapFrom(x => x.image))
            .ForMember(o => o.Nds, o => o.MapFrom(x => x.nds))
            .ForMember(o => o.RemoteUrl, o => o.MapFrom(x => x.remote_uri))
            .ForMember(o => o.RemoteUrlMethodId, o => o.MapFrom(x => x.remote_uri_method_id))
            .ForMember(o => o.RemoteUrlMethodName, o => o.MapFrom(x => x.remote_uri_method_name))
            .ForMember(o => o.EmailSearchCriteria_FileNamePattern, o => o.MapFrom(x => x.email_criteria_file_name_pattern))
            .ForMember(o => o.EmailSearchCriteria_MaxAgeInDays, o => o.MapFrom(x => x.email_criteria_max_age_in_days))
            .ForMember(o => o.EmailSearchCriteria_Sender, o => o.MapFrom(x => x.email_criteria_sender))
            .ForMember(o => o.EmailSearchCriteria_Subject, o => o.MapFrom(x => x.email_criteria_subject))
            .ForMember(o => o.Cridentials_Login, o => o.MapFrom(x => x.credentials_login))
            .ForMember(o => o.Cridentials_Password, o => o.MapFrom(x => x.credentials_password))
            .ForMember(o => o.LinkedStockId, o => o.MapFrom(x => x.stock_partner_id))
            .ForMember(o => o.QuantityMap, o => o.MapFrom(x => x.quantity_map.ToDictionary(i => i.text, i => i.quantity)))
            .ForMember(o => o.ManufacturerNameMap, o => o.MapFrom(x => x.manufacturer_name_map.ToDictionary(i => i.text, i => i.name)));
        // заполнен не до конца, вынести Email Search Criteria, Credentials и т.д. в подобьекты
    }
}

