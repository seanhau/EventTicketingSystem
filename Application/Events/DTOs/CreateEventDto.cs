using System;

namespace Application.Events.DTOs;

public class CreateEventDto : BaseEventDto
{
    public List<PricingTierDto> PricingTiers { get; set; } = new();
}

public class PricingTierDto
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
}


