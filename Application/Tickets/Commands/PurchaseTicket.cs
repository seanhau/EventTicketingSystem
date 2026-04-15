using System;
using Application.Core;
using Application.Tickets.DTOs;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tickets.Commands;

public class PurchaseTicket
{
    public class Command : IRequest<Result<TicketPurchaseResponseDto>>
    {
        public required PurchaseTicketDto PurchaseDto { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<TicketPurchaseResponseDto>>
    {
        public async Task<Result<TicketPurchaseResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Load event with pricing tiers and existing purchases
            var eventEntity = await context.Events
                .Include(e => e.PricingTiers)
                    .ThenInclude(pt => pt.TicketPurchases)
                .Include(e => e.TicketPurchases)
                .FirstOrDefaultAsync(e => e.Id == request.PurchaseDto.EventId, cancellationToken);

            if (eventEntity == null)
                return Result<TicketPurchaseResponseDto>.Failure("Event not found", 404);

            if (eventEntity.IsCancelled)
                return Result<TicketPurchaseResponseDto>.Failure("Event is cancelled", 400);

            if (eventEntity.Date < DateTime.UtcNow.Date)
                return Result<TicketPurchaseResponseDto>.Failure("Cannot purchase tickets for past events", 400);

            var pricingTier = eventEntity.PricingTiers
                .FirstOrDefault(pt => pt.Id == request.PurchaseDto.PricingTierId);

            if (pricingTier == null)
                return Result<TicketPurchaseResponseDto>.Failure("Pricing tier not found", 404);

            // Check availability for this pricing tier
            var availableInTier = pricingTier.AvailableTickets;
            if (availableInTier < request.PurchaseDto.Quantity)
            {
                return Result<TicketPurchaseResponseDto>.Failure(
                    $"Only {availableInTier} tickets available in {pricingTier.Name} tier",
                    400);
            }

            // Check overall event capacity
            var availableInEvent = eventEntity.AvailableTickets;
            if (availableInEvent < request.PurchaseDto.Quantity)
            {
                return Result<TicketPurchaseResponseDto>.Failure(
                    $"Only {availableInEvent} tickets available for this event",
                    400);
            }

            // Create ticket purchase
            var totalPrice = pricingTier.Price * request.PurchaseDto.Quantity;
            var confirmationCode = GenerateConfirmationCode();

            var ticketPurchase = new TicketPurchase
            {
                EventId = request.PurchaseDto.EventId,
                PricingTierId = request.PurchaseDto.PricingTierId,
                CustomerName = request.PurchaseDto.CustomerName,
                CustomerEmail = request.PurchaseDto.CustomerEmail,
                Quantity = request.PurchaseDto.Quantity,
                TotalPrice = totalPrice,
                ConfirmationCode = confirmationCode,
                PurchasedAt = DateTime.UtcNow
            };

            context.TicketPurchases.Add(ticketPurchase);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
                return Result<TicketPurchaseResponseDto>.Failure("Failed to complete ticket purchase", 400);

            var response = new TicketPurchaseResponseDto
            {
                PurchaseId = ticketPurchase.Id,
                ConfirmationCode = confirmationCode,
                EventName = eventEntity.Name,
                PricingTierName = pricingTier.Name,
                Quantity = request.PurchaseDto.Quantity,
                TotalPrice = totalPrice,
                PurchasedAt = ticketPurchase.PurchasedAt
            };

            return Result<TicketPurchaseResponseDto>.Success(response);
        }

        private static string GenerateConfirmationCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}


