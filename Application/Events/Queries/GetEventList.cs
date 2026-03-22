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

            // Load all events first
            var events = await query.ToListAsync(cancellationToken);

            // Sort by date and time in memory (client-side evaluation)
            // Convert TimeSpan to Ticks for reliable comparison
            var sortedEvents = events
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time.Ticks)
                .ThenBy(e => e.Id)
                .ToList();

            return sortedEvents.Select(e => new EventDetailsDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Venue = e.Venue,
                Date = e.Date,
                Time = e.Time,
                TotalTicketCapacity = e.TotalTicketCapacity,
                AvailableTickets = e.AvailableTickets,
                IsCancelled = e.IsCancelled,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                PricingTiers = e.PricingTiers.Select(pt => new PricingTierDetailsDto
                {
                    Id = pt.Id,
                    Name = pt.Name,
                    Price = pt.Price,
                    Capacity = pt.Capacity,
                    AvailableTickets = pt.AvailableTickets
                }).ToList()
            }).ToList();
        }
    }
}


