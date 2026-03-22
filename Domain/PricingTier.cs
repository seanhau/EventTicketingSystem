using System;

namespace Domain;

public class PricingTier
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventId { get; set; } = string.Empty;
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Event? Event { get; set; }
    public ICollection<TicketPurchase> TicketPurchases { get; set; } = new List<TicketPurchase>();

    // Computed property
    public int AvailableTickets => Capacity - TicketPurchases.Sum(tp => tp.Quantity);
}


