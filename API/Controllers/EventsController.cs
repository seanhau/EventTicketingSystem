using Application.Events.Commands;
using Application.Events.DTOs;
using Application.Events.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Manages events and ticket sales
/// </summary>
public class EventsController : BaseApiController
{
    /// <summary>
    /// Get a list of all events
    /// </summary>
    /// <param name="includePastEvents">Include events that have already occurred</param>
    /// <param name="includeCancelled">Include cancelled events</param>
    /// <returns>List of events with pricing tier details</returns>
    /// <response code="200">Returns the list of events</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventDetailsDto>>> GetEvents(
        [FromQuery] bool includePastEvents = false,
        [FromQuery] bool includeCancelled = false)
    {
        return await Mediator.Send(new GetEventList.Query
        {
            IncludePastEvents = includePastEvents,
            IncludeCancelled = includeCancelled
        });
    }

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>Event details including pricing tiers and availability</returns>
    /// <response code="200">Returns the event details</response>
    /// <response code="404">Event not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailsDto>> GetEvent(string id)
    {
        return HandleResult(await Mediator.Send(new GetEventDetails.Query { Id = id }));
    }

    /// <summary>
    /// Create a new event with pricing tiers
    /// </summary>
    /// <param name="eventDto">Event creation details</param>
    /// <returns>The created event ID</returns>
    /// <response code="201">Event created successfully</response>
    /// <response code="400">Invalid event data (capacity mismatch, duplicate tier names, etc.)</response>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEvent(CreateEventDto eventDto)
    {
        var result = await Mediator.Send(new CreateEvent.Command { EventDto = eventDto });
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetEvent), new { id = result.Value }, result.Value);
        }
        
        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing event
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <param name="eventDto">Updated event details</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Event updated successfully</response>
    /// <response code="400">Invalid data or ID mismatch</response>
    /// <response code="404">Event not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(string id, UpdateEventDto eventDto)
    {
        if (id != eventDto.Id)
        {
            return BadRequest("ID mismatch");
        }

        return HandleResult(await Mediator.Send(new UpdateEvent.Command { EventDto = eventDto }));
    }

    /// <summary>
    /// Delete an event
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Event deleted successfully</response>
    /// <response code="404">Event not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteEvent.Command { Id = id }));
    }
}

