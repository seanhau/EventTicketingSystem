using System;
using Application.Events.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Events.Queries;

public class GetEventList
{
    public class Query : IRequest<List<EventDetailsDto>>
    {
        public bool IncludePastEvents { get; set; } = false;
        public bool IncludeCancelled { get; set; } = false;
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, List<EventDetailsDto>>
    {
        public async Task<List<EventDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Build base query with filters
            var query = context.Events.AsQueryable();

            if (!request.IncludePastEvents)
            {
                query = query.Where(e => e.Date >= DateTime.UtcNow.Date);
            }

            if (!request.IncludeCancelled)
            {
                query = query.Where(e => !e.IsCancelled);
            }

            // Project to DTO with database-side calculation of available tickets
            // This avoids loading all TicketPurchases into memory
            var events = await query
                .Select(e => new EventDetailsDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Venue = e.Venue,
                    Date = e.Date,
                    Time = e.Time,
                    TotalTicketCapacity = e.TotalTicketCapacity,
                    // Calculate available tickets in database
                    AvailableTickets = e.TotalTicketCapacity - e.TicketPurchases.Sum(tp => tp.Quantity),
                    IsCancelled = e.IsCancelled,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    PricingTiers = e.PricingTiers.Select(pt => new PricingTierDetailsDto
                    {
                        Id = pt.Id,
                        Name = pt.Name,
                        Price = pt.Price,
                        Capacity = pt.Capacity,
                        // Calculate available tickets for each tier in database
                        AvailableTickets = pt.Capacity - pt.TicketPurchases.Sum(tp => tp.Quantity)
                    }).ToList()
                })
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time)
                .ToListAsync(cancellationToken);

            return events;
        }
    }
}


