using System;
using Application.Tickets.Commands;
using FluentValidation;

namespace Application.Tickets.Validators;

public sealed class PurchaseTicketValidator : AbstractValidator<PurchaseTicket.Command>
{
    public PurchaseTicketValidator()
    {
        RuleFor(x => x.PurchaseDto.EventId)
            .NotEmpty().WithMessage("Event ID is required");

        RuleFor(x => x.PurchaseDto.PricingTierId)
            .NotEmpty().WithMessage("Pricing tier ID is required");

        RuleFor(x => x.PurchaseDto.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters");

        RuleFor(x => x.PurchaseDto.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(200).WithMessage("Customer email must not exceed 200 characters");

        RuleFor(x => x.PurchaseDto.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1")
            .LessThanOrEqualTo(10).WithMessage("Maximum 10 tickets per purchase");
    }
}


