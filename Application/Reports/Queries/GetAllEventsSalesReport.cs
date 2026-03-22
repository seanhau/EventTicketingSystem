using System;
using Application.Reports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Reports.Queries;

public class GetAllEventsSalesReport
{
    public class Query : IRequest<List<EventSalesReportDto>>
    {
        public bool IncludePastEvents { get; set; } = true;
        public bool IncludeCancelled { get; set; } = false;
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, List<EventSalesReportDto>>
    {
        public async Task<List<EventSalesReportDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Events
                .Include(e => e.PricingTiers)
                    .ThenInclude(pt => pt.TicketPurchases)
                .Include(e => e.TicketPurchases)
                .AsQueryable();

            if (!request.IncludePastEvents)
            {
                query = query.Where(e => e.Date >= DateTime.UtcNow.Date);
            }

            if (!request.IncludeCancelled)
            {
                query = query.Where(e => !e.IsCancelled);
            }

            var events = await query
                .OrderBy(e => e.Date)
                .ToListAsync(cancellationToken);

            var reports = events.Select(e =>
            {
                var totalTicketsSold = e.TicketPurchases.Sum(tp => tp.Quantity);
                var totalRevenue = e.TicketPurchases.Sum(tp => tp.TotalPrice);

                return new EventSalesReportDto
                {
                    EventId = e.Id,
                    EventName = e.Name,
                    EventDate = e.Date,
                    TotalTicketsSold = totalTicketsSold,
                    TotalTicketsAvailable = e.AvailableTickets,
                    TotalRevenue = totalRevenue,
                    PricingTierSales = e.PricingTiers.Select(pt => new PricingTierSalesDto
                    {
                        PricingTierName = pt.Name,
                        Price = pt.Price,
                        TicketsSold = pt.TicketPurchases.Sum(tp => tp.Quantity),
                        TicketsAvailable = pt.AvailableTickets,
                        Revenue = pt.TicketPurchases.Sum(tp => tp.TotalPrice)
                    }).ToList()
                };
            }).ToList();

            return reports;
        }
    }
}


