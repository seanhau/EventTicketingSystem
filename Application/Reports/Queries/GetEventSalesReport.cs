using System;
using Application.Core;
using Application.Reports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Reports.Queries;

public class GetEventSalesReport
{
    public class Query : IRequest<Result<EventSalesReportDto>>
    {
        public required string EventId { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, Result<EventSalesReportDto>>
    {
        public async Task<Result<EventSalesReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var eventEntity = await context.Events
                .Include(e => e.PricingTiers)
                    .ThenInclude(pt => pt.TicketPurchases)
                .Include(e => e.TicketPurchases)
                .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

            if (eventEntity == null)
                return Result<EventSalesReportDto>.Failure("Event not found", 404);

            var totalTicketsSold = eventEntity.TicketPurchases.Sum(tp => tp.Quantity);
            var totalRevenue = eventEntity.TicketPurchases.Sum(tp => tp.TotalPrice);

            var pricingTierSales = eventEntity.PricingTiers.Select(pt => new PricingTierSalesDto
            {
                PricingTierName = pt.Name,
                Price = pt.Price,
                TicketsSold = pt.TicketPurchases.Sum(tp => tp.Quantity),
                TicketsAvailable = pt.AvailableTickets,
                Revenue = pt.TicketPurchases.Sum(tp => tp.TotalPrice)
            }).ToList();

            var report = new EventSalesReportDto
            {
                EventId = eventEntity.Id,
                EventName = eventEntity.Name,
                EventDate = eventEntity.Date,
                TotalTicketsSold = totalTicketsSold,
                TotalTicketsAvailable = eventEntity.AvailableTickets,
                TotalRevenue = totalRevenue,
                PricingTierSales = pricingTierSales
            };

            return Result<EventSalesReportDto>.Success(report);
        }
    }
}


