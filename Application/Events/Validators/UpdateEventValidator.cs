using System;
using Application.Events.Commands;
using Application.Events.DTOs;
using FluentValidation;

namespace Application.Events.Validators;

public sealed class UpdateEventValidator : BaseEventValidator<UpdateEvent.Command, UpdateEventDto>
{
    public UpdateEventValidator() : base(x => x.EventDto)
    {
        RuleFor(x => x.EventDto.Id)
            .NotEmpty().WithMessage("Event ID is required");
    }
}


