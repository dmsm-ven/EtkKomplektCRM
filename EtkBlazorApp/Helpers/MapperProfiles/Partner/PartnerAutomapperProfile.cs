using AutoMapper;
using EtkBlazorApp.DataAccess.Entity;
using System;

namespace EtkBlazorApp.Helpers.MapperProfiles;

public class PartnerAutomapperProfile : Profile
{
    public PartnerAutomapperProfile()
    {
        CreateMap<PartnerEntity, PartnerViewModel>()
            .ForMember(x => x.Id, x => x.MapFrom(p => Guid.Parse(p.id)))
            .ForMember(x => x.Address, x => x.MapFrom(p => p.address))
            .ForMember(x => x.ContactPerson, x => x.MapFrom(p => p.contact_person))
            .ForMember(x => x.Created, x => x.MapFrom(p => p.created))
            .ForMember(x => x.Description, x => x.MapFrom(p => p.description))
            .ForMember(x => x.Discount, x => x.MapFrom(p => p.discount))
            .ForMember(x => x.Email, x => x.MapFrom(p => p.email))
            .ForMember(x => x.Name, x => x.MapFrom(p => p.name))
            .ForMember(x => x.Password, x => x.MapFrom(p => p.price_list_password))
            .ForMember(x => x.PhoneNumber, x => x.MapFrom(p => p.phone_number))
            .ForMember(x => x.PriceListLastAccessDateTime, x => x.MapFrom(p => p.price_list_last_access))
            .ForMember(x => x.Priority, x => x.MapFrom(p => p.priority))
            .ForMember(x => x.Updated, x => x.MapFrom(p => p.updated))
            .ForMember(x => x.Website, x => x.MapFrom(p => p.website));
    }
}

