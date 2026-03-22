using System;
using Application.Core;
using Application.Events.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tickets.Queries;

public class GetTicketAvailability
{
    public class Query : IRequest<Result<EventDetailsDto>>
    {
        public required string EventId { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, Result<EventDetailsDto>>
    {
        public async Task<Result<EventDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var eventEntity = await context.Events
                .Include(e => e.PricingTiers)
                    .ThenInclude(pt => pt.TicketPurchases)
                .Include(e => e.TicketPurchases)
                .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

            if (eventEntity == null)
                return Result<EventDetailsDto>.Failure("Event not found", 404);

            var dto = new EventDetailsDto
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                Venue = eventEntity.Venue,
                Date = eventEntity.Date,
                Time = eventEntity.Time,
                TotalTicketCapacity = eventEntity.TotalTicketCapacity,
                AvailableTickets = eventEntity.AvailableTickets,
                IsCancelled = eventEntity.IsCancelled,
                CreatedAt = eventEntity.CreatedAt,
                UpdatedAt = eventEntity.UpdatedAt,
                PricingTiers = eventEntity.PricingTiers.Select(pt => new PricingTierDetailsDto
                {
                    Id = pt.Id,
                    Name = pt.Name,
                    Price = pt.Price,
                    Capacity = pt.Capacity,
                    AvailableTickets = pt.AvailableTickets
                }).ToList()
            };

            return Result<EventDetailsDto>.Success(dto);
        }
    }
}


