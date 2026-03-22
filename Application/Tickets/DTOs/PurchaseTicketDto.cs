using System;

namespace Application.Tickets.DTOs;

public class PurchaseTicketDto
{
    public required string EventId { get; set; }
    public required string PricingTierId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public int Quantity { get; set; }
}


