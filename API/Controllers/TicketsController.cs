using Application.Events.DTOs;
using Application.Tickets.Commands;
using Application.Tickets.DTOs;
using Application.Tickets.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TicketsController : BaseApiController
{
    /// <summary>
    /// Purchase tickets for an event
    /// </summary>
    /// <param name="purchaseDto">The ticket purchase details including event ID, pricing tier, customer information, and quantity</param>
    /// <returns>Ticket purchase confirmation with details</returns>
    /// <response code="200">Ticket purchased successfully</response>
    /// <response code="400">Invalid purchase request (e.g., insufficient tickets available, invalid pricing tier)</response>
    /// <response code="404">Event or pricing tier not found</response>
    [HttpPost("purchase")]
    public async Task<ActionResult<TicketPurchaseResponseDto>> PurchaseTicket(PurchaseTicketDto purchaseDto)
    {
        return HandleResult(await Mediator.Send(new PurchaseTicket.Command { PurchaseDto = purchaseDto }));
    }

    /// <summary>
    /// Get ticket availability for an event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>Event details including available tickets for each pricing tier</returns>
    /// <response code="200">Returns the event details with ticket availability</response>
    /// <response code="404">Event not found</response>
    [HttpGet("availability/{eventId}")]
    public async Task<ActionResult<EventDetailsDto>> GetTicketAvailability(string eventId)
    {
        return HandleResult(await Mediator.Send(new GetTicketAvailability.Query { EventId = eventId }));
    }
}

