using System;

namespace Domain;

public class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Venue { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int TotalTicketCapacity { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();
    public ICollection<TicketPurchase> TicketPurchases { get; set; } = new List<TicketPurchase>();

    // Computed property
    public int AvailableTickets => TotalTicketCapacity - TicketPurchases.Sum(tp => tp.Quantity);
}


