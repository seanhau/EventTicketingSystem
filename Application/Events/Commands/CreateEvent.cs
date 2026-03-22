using System;
using Application.Core;
using Application.Events.DTOs;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Events.Commands;

public class CreateEvent
{
    public class Command : IRequest<Result<string>>
    {
        public required CreateEventDto EventDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validate total capacity matches sum of pricing tier capacities
            var totalPricingCapacity = request.EventDto.PricingTiers.Sum(pt => pt.Capacity);
            if (totalPricingCapacity != request.EventDto.TotalTicketCapacity)
            {
                return Result<string>.Failure(
                    "Total ticket capacity must equal the sum of all pricing tier capacities", 
                    400);
            }

            // Check for duplicate pricing tier names
            var duplicateTiers = request.EventDto.PricingTiers
                .GroupBy(pt => pt.Name.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateTiers.Any())
            {
                return Result<string>.Failure(
                    $"Duplicate pricing tier names found: {string.Join(", ", duplicateTiers)}", 
                    400);
            }

            var eventEntity = mapper.Map<Event>(request.EventDto);
            eventEntity.PricingTiers = request.EventDto.PricingTiers
                .Select(pt => new PricingTier
                {
                    EventId = eventEntity.Id,
                    Name = pt.Name,
                    Price = pt.Price,
                    Capacity = pt.Capacity
                })
                .ToList();

            context.Events.Add(eventEntity);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) 
                return Result<string>.Failure("Failed to create the event", 400);

            return Result<string>.Success(eventEntity.Id);
        }
    }
}


