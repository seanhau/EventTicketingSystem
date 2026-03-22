using System;
using Application.Events.DTOs;
using AutoMapper;
using Domain;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Event mappings
        CreateMap<CreateEventDto, Event>()
            .ForMember(dest => dest.PricingTiers, opt => opt.Ignore());
        CreateMap<UpdateEventDto, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.PricingTiers, opt => opt.Ignore())
            .ForMember(dest => dest.TicketPurchases, opt => opt.Ignore());
    }
}
