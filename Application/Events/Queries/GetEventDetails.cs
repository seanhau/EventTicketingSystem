using System;
using Application.Core;
using Application.Events.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Events.Queries;

public class GetEventDetails
{
    public class Query : IRequest<Result<EventDetailsDto>>
    {
        public required string Id { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, Result<EventDetailsDto>>
    {
        public async Task<Result<EventDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var eventEntity = await context.Events
                .Include(e => e.PricingTiers)
                    .ThenInclude(pt => pt.TicketPurchases)
                .Include(e => e.TicketPurchases)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

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


