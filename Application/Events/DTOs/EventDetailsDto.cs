using System;

namespace Application.Events.DTOs;

public class EventDetailsDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Venue { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int TotalTicketCapacity { get; set; }
    public int AvailableTickets { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PricingTierDetailsDto> PricingTiers { get; set; } = new();
}

public class PricingTierDetailsDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public int AvailableTickets { get; set; }
}


