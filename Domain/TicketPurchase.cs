using System;

namespace Domain;

public class TicketPurchase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string EventId { get; set; }
    public required string PricingTierId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public string? ConfirmationCode { get; set; }

    // Navigation properties
    public Event? Event { get; set; }
    public PricingTier? PricingTier { get; set; }
}


