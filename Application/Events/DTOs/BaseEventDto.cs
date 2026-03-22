using System;

namespace Application.Events.DTOs;

public abstract class BaseEventDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Venue { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int TotalTicketCapacity { get; set; }
}


