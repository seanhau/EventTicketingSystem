using Application.Tickets.Commands;
using Application.Tickets.DTOs;
using Application.Tickets.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.Tests.Application.Validators;

public class PurchaseTicketValidatorTests
{
    private readonly PurchaseTicketValidator _validator;

    public PurchaseTicketValidatorTests()
    {
        _validator = new PurchaseTicketValidator();
    }

    [Fact]
    public void Should_HaveError_WhenEventIdIsEmpty()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = string.Empty,
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = 2
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.EventId);
    }

    [Fact]
    public void Should_HaveError_WhenPricingTierIdIsEmpty()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = string.Empty,
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = 2
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.PricingTierId);
    }

    [Fact]
    public void Should_HaveError_WhenCustomerNameIsEmpty()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = string.Empty,
                CustomerEmail = "john@example.com",
                Quantity = 2
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.CustomerName);
    }

    [Fact]
    public void Should_HaveError_WhenCustomerNameExceedsMaxLength()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = new string('A', 201), // 201 characters
                CustomerEmail = "john@example.com",
                Quantity = 2
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.CustomerName);
    }

    [Fact]
    public void Should_HaveError_WhenCustomerEmailIsEmpty()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = string.Empty,
                Quantity = 2
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.CustomerEmail);
    }

    [Fact]
    public void Should_HaveError_WhenCustomerEmailIsInvalid()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "invalid-email",
                Quantity = 2
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.CustomerEmail);
    }

    [Fact]
    public void Should_HaveError_WhenCustomerEmailExceedsMaxLength()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = new string('a', 190) + "@example.com", // 201 characters
                Quantity = 2
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.CustomerEmail);
    }

    [Fact]
    public void Should_HaveError_WhenQuantityIsZero()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = 0
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.Quantity);
    }

    [Fact]
    public void Should_HaveError_WhenQuantityIsNegative()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = -1
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.Quantity);
    }

    [Fact]
    public void Should_HaveError_WhenQuantityExceedsMaximum()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = 11 // Maximum is 10
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDto.Quantity);
    }

    [Fact]
    public void Should_NotHaveError_WhenValidCommand()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = 5
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenQuantityIsOne()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = 1
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_NotHaveError_WhenQuantityIsTen()
    {
        var command = new PurchaseTicket.Command
        {
            PurchaseDto = new PurchaseTicketDto
            {
                EventId = Guid.NewGuid().ToString(),
                PricingTierId = Guid.NewGuid().ToString(),
                CustomerName = "John Doe",
                CustomerEmail = "john@example.com",
                Quantity = 10
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}


