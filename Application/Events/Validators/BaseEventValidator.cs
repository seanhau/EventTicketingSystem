using System;
using Application.Events.DTOs;
using FluentValidation;

namespace Application.Events.Validators;

public class BaseEventValidator<T, TDto> : AbstractValidator<T> where TDto : BaseEventDto
{
    public BaseEventValidator(Func<T, TDto> selector)
    {
        RuleFor(x => selector(x).Name)
            .NotEmpty().WithMessage("Event name is required")
            .MaximumLength(200).WithMessage("Event name must not exceed 200 characters");

        RuleFor(x => selector(x).Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => selector(x).Venue)
            .NotEmpty().WithMessage("Venue is required")
            .MaximumLength(300).WithMessage("Venue must not exceed 300 characters");

        RuleFor(x => selector(x).Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Event date must be today or in the future");

        RuleFor(x => selector(x).TotalTicketCapacity)
            .GreaterThan(0).WithMessage("Total ticket capacity must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Total ticket capacity cannot exceed 100,000");
    }
}


