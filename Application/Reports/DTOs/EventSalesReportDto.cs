using System;

namespace Application.Reports.DTOs;

public class EventSalesReportDto
{
    public required string EventId { get; set; }
    public required string EventName { get; set; }
    public DateTime EventDate { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalTicketsAvailable { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<PricingTierSalesDto> PricingTierSales { get; set; } = new();
}

public class PricingTierSalesDto
{
    public required string PricingTierName { get; set; }
    public decimal Price { get; set; }
    public int TicketsSold { get; set; }
    public int TicketsAvailable { get; set; }
    public decimal Revenue { get; set; }
}


