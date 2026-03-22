using System;

namespace Application.Events.DTOs;

public class UpdateEventDto : BaseEventDto
{
    public required string Id { get; set; }
    public bool IsCancelled { get; set; }
}


