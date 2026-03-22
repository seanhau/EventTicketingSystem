using Application.Events.Commands;
using Application.Events.DTOs;
using Application.Events.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Application.Validators;

public class CreateEventValidatorTests
{
    private readonly CreateEventValidator _validator;

    public CreateEventValidatorTests()
    {
        _validator = new CreateEventValidator();
    }

    [Fact]
    public void Should_HaveError_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventDto.Name);
    }

    [Fact]
    public void Should_HaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = new string('A', 201),
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventDto.Name);
    }

    [Fact]
    public void Should_HaveError_WhenDescriptionIsEmpty()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventDto.Description);
    }

    [Fact]
    public void Should_HaveError_WhenDateIsInPast()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(-1),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventDto.Date);
    }

    [Fact]
    public void Should_HaveError_WhenTotalTicketCapacityIsZero()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 0,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventDto.TotalTicketCapacity);
    }

    [Fact]
    public void Should_HaveError_WhenNoPricingTiers()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>()
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventDto.PricingTiers);
    }

    [Fact]
    public void Should_HaveError_WhenTooManyPricingTiers()
    {
        // Arrange
        var pricingTiers = new List<PricingTierDto>();
        for (int i = 0; i < 11; i++)
        {
            pricingTiers.Add(new PricingTierDto
            {
                Name = $"Tier {i}",
                Price = 50.00m,
                Capacity = 10
            });
        }

        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 110,
                PricingTiers = pricingTiers
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventDto.PricingTiers);
    }

    [Fact]
    public void Should_HaveError_WhenPricingTierNameIsEmpty()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "", Price = 50.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("EventDto.PricingTiers[0].Name");
    }

    [Fact]
    public void Should_HaveError_WhenPricingTierPriceIsNegative()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = -10.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("EventDto.PricingTiers[0].Price");
    }

    [Fact]
    public void Should_HaveError_WhenPricingTierCapacityIsZero()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Event Name",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 0 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("EventDto.PricingTiers[0].Capacity");
    }

    [Fact]
    public void Should_NotHaveError_WhenValidCommand()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Valid Event",
                Description = "Valid description",
                Venue = "Valid Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 30, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "VIP", Price = 150.00m, Capacity = 30 },
                    new() { Name = "General", Price = 50.00m, Capacity = 70 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_AllowFreePricingTier()
    {
        // Arrange
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Free Event",
                Description = "Free event description",
                Venue = "Community Center",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(14, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Free Admission", Price = 0.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor("EventDto.PricingTiers[0].Price");
    }
}

