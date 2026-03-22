using System;

namespace Application.Tickets.DTOs;

public class TicketPurchaseResponseDto
{
    public required string PurchaseId { get; set; }
    public required string ConfirmationCode { get; set; }
    public required string EventName { get; set; }
    public required string PricingTierName { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime PurchasedAt { get; set; }
}


