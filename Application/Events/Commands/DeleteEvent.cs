using System;
using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Events.Commands;

public class DeleteEvent
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string Id { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var eventEntity = await context.Events
                .Include(e => e.TicketPurchases)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (eventEntity == null)
                return Result<Unit>.Failure("Event not found", 404);

            // Prevent deletion if tickets have been sold
            if (eventEntity.TicketPurchases.Any())
            {
                return Result<Unit>.Failure(
                    "Cannot delete event with existing ticket purchases. Consider cancelling the event instead.", 
                    400);
            }

            context.Events.Remove(eventEntity);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                return Result<Unit>.Failure("Failed to delete the event", 400);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}


