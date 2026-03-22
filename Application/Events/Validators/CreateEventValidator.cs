using System;
using Application.Events.Commands;
using Application.Events.DTOs;
using FluentValidation;

namespace Application.Events.Validators;

public sealed class CreateEventValidator : BaseEventValidator<CreateEvent.Command, CreateEventDto>
{
    public CreateEventValidator() : base(x => x.EventDto)
    {
        RuleFor(x => x.EventDto.PricingTiers)
            .NotEmpty().WithMessage("At least one pricing tier is required")
            .Must(tiers => tiers.Count <= 10).WithMessage("Maximum 10 pricing tiers allowed");

        RuleForEach(x => x.EventDto.PricingTiers).ChildRules(tier =>
        {
            tier.RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Pricing tier name is required")
                .MaximumLength(100).WithMessage("Pricing tier name must not exceed 100 characters");

            tier.RuleFor(t => t.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or greater")
                .LessThanOrEqualTo(10000).WithMessage("Price cannot exceed 10,000");

            tier.RuleFor(t => t.Capacity)
                .GreaterThan(0).WithMessage("Pricing tier capacity must be greater than 0");
        });
    }
}


