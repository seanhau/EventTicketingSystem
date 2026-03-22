using Application.Events.Commands;
using Application.Events.DTOs;
using Application.Events.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.Tests.Application.Validators;

public class UpdateEventValidatorTests
{
    private readonly UpdateEventValidator _validator;

    public UpdateEventValidatorTests()
    {
        _validator = new UpdateEventValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        var command = new UpdateEvent.Command
        {
            EventDto = new UpdateEventDto
            {
                Id = string.Empty,
                Name = "Valid Event",
                Description = "Valid Description",
                Venue = "Valid Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(18, 0, 0),
                TotalTicketCapacity = 100,
                IsCancelled = false
            }
        };

        var result = _validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_HaveError_WhenNameIsEmpty()
    {
        var command = new UpdateEvent.Command
        {
            EventDto = new UpdateEventDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = string.Empty,
                Description = "Valid Description",
                Venue = "Valid Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(18, 0, 0),
                TotalTicketCapacity = 100,
                IsCancelled = false
            }
        };

        var result = _validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_HaveError_WhenDateIsInPast()
    {
        var command = new UpdateEvent.Command
        {
            EventDto = new UpdateEventDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Valid Event",
                Description = "Valid Description",
                Venue = "Valid Venue",
                Date = DateTime.UtcNow.AddDays(-1),
                Time = new TimeSpan(18, 0, 0),
                TotalTicketCapacity = 100,
                IsCancelled = false
            }
        };

        var result = _validator.TestValidate(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_NotHaveError_WhenValidCommand()
    {
        var command = new UpdateEvent.Command
        {
            EventDto = new UpdateEventDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Valid Event",
                Description = "Valid Description",
                Venue = "Valid Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(18, 0, 0),
                TotalTicketCapacity = 100,
                IsCancelled = false
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}


