using AutoMapper;
using EtkBlazorApp.DataAccess;
using EtkBlazorApp.DataAccess.Entity;
using EtkBlazorApp.DataAccess.Entity.PriceList;
using EtkBlazorApp.Model.PriceListTemplate;
using System;
using System.Linq;

namespace EtkBlazorApp.Helpers.MapperProfiles.PriceList;

public class PriceListProfile : Profile
{
    public PriceListProfile()
    {
        CreateMap<ManufacturerSkipRecordEntity, ManufacturerSkipItemViewModel>()
            .ForMember(o => o.manufacturer_id, o => o.MapFrom(x => x.manufacturer_id))
            .ForMember(o => o.Name, o => o.MapFrom(x => x.name))
            .ForMember(o => o.ListType, o => o.MapFrom(x => Enum.Parse<SkipManufacturerListType>(x.list_type)));

        CreateMap<ManufacturerDiscountMapEntity, ManufacturerDiscountItemViewModel>()
            .ForMember(x => x.manufacturer_id, x => x.MapFrom(m => m.manufacturer_id))
            .ForMember(x => x.manufacturer_name, x => x.MapFrom(m => m.name))
            .ForMember(x => x.discount, x => x.MapFrom(m => m.discount));


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
            .ForMember(o => o.ManufacturerNameMap, o => o.MapFrom(x => x.manufacturer_name_map.ToDictionary(i => i.text, i => i.name)))
            .ForMember(o => o.ModelMap, o => o.MapFrom(x => x.model_map.ToDictionary(i => i.old_text, i => i.new_text)))
            .ForMember(o => o.ManufacturerDiscountMap, o => o.MapFrom(x => x.manufacturer_discount_map))
            .ForMember(o => o.ManufacturerPurchaseDiscountMap, o => o.MapFrom(x => x.manufacturer_purchase_map))
            .ForMember(o => o.ManufacturerSkipList, o => o.MapFrom(x => x.manufacturer_skip_list));
    }
}

