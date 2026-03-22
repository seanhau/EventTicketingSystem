using Application.Reports.DTOs;
using Application.Reports.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ReportsController : BaseApiController
{
    /// <summary>
    /// Get sales reports for all events
    /// </summary>
    /// <param name="includePastEvents">Include events that have already occurred (default: true)</param>
    /// <param name="includeCancelled">Include cancelled events (default: false)</param>
    /// <returns>List of event sales reports</returns>
    /// <response code="200">Returns the list of sales reports</response>
    [HttpGet("sales")]
    public async Task<ActionResult<List<EventSalesReportDto>>> GetAllEventsSalesReport(
        [FromQuery] bool includePastEvents = true,
        [FromQuery] bool includeCancelled = false)
    {
        return await Mediator.Send(new GetAllEventsSalesReport.Query
        {
            IncludePastEvents = includePastEvents,
            IncludeCancelled = includeCancelled
        });
    }

    /// <summary>
    /// Get sales report for a specific event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>Sales report for the specified event</returns>
    /// <response code="200">Returns the sales report</response>
    /// <response code="404">Event not found</response>
    [HttpGet("sales/{eventId}")]
    public async Task<ActionResult<EventSalesReportDto>> GetEventSalesReport(string eventId)
    {
        return HandleResult(await Mediator.Send(new GetEventSalesReport.Query { EventId = eventId }));
    }
}

