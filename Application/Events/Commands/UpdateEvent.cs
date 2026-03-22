using System;
using Application.Core;
using Application.Events.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Events.Commands;

public class UpdateEvent
{
    public class Command : IRequest<Result<Unit>>
    {
        public required UpdateEventDto EventDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var eventEntity = await context.Events
                .Include(e => e.TicketPurchases)
                .FirstOrDefaultAsync(e => e.Id == request.EventDto.Id, cancellationToken);

            if (eventEntity == null)
                return Result<Unit>.Failure("Event not found", 404);

            // Prevent reducing capacity below tickets already sold
            var ticketsSold = eventEntity.TicketPurchases.Sum(tp => tp.Quantity);
            if (request.EventDto.TotalTicketCapacity < ticketsSold)
            {
                return Result<Unit>.Failure(
                    $"Cannot reduce capacity below {ticketsSold} tickets already sold", 
                    400);
            }

            mapper.Map(request.EventDto, eventEntity);
            eventEntity.UpdatedAt = DateTime.UtcNow;

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                return Result<Unit>.Failure("Failed to update the event", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}


