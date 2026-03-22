using Domain;
using FluentAssertions;

namespace Application.Tests.Domain;

public class EventTests
{
    [Fact]
    public void Event_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var eventEntity = new Event
        {
            Name = "Test Event",
            Description = "Test Description",
            Venue = "Test Venue"
        };

        // Assert
        eventEntity.Id.Should().NotBeNullOrEmpty();
        eventEntity.IsCancelled.Should().BeFalse();
        eventEntity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        eventEntity.UpdatedAt.Should().BeNull();
        eventEntity.PricingTiers.Should().NotBeNull().And.BeEmpty();
        eventEntity.TicketPurchases.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AvailableTickets_ShouldReturnTotalCapacity_WhenNoPurchases()
    {
        // Arrange
        var eventEntity = new Event
        {
            Name = "Test Event",
            Description = "Test Description",
            Venue = "Test Venue",
            TotalTicketCapacity = 100
        };

        // Act
        var availableTickets = eventEntity.AvailableTickets;

        // Assert
        availableTickets.Should().Be(100);
    }

    [Fact]
    public void AvailableTickets_ShouldDeductPurchasedQuantity()
    {
        // Arrange
        var eventEntity = new Event
        {
            Name = "Test Event",
            Description = "Test Description",
            Venue = "Test Venue",
            TotalTicketCapacity = 100
        };

        eventEntity.TicketPurchases.Add(new TicketPurchase
        {
            EventId = eventEntity.Id,
            PricingTierId = "tier1",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 10,
            TotalPrice = 100
        });

        eventEntity.TicketPurchases.Add(new TicketPurchase
        {
            EventId = eventEntity.Id,
            PricingTierId = "tier1",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            Quantity = 15,
            TotalPrice = 150
        });

        // Act
        var availableTickets = eventEntity.AvailableTickets;

        // Assert
        availableTickets.Should().Be(75);
    }

    [Fact]
    public void AvailableTickets_ShouldReturnZero_WhenFullyBooked()
    {
        // Arrange
        var eventEntity = new Event
        {
            Name = "Test Event",
            Description = "Test Description",
            Venue = "Test Venue",
            TotalTicketCapacity = 50
        };

        eventEntity.TicketPurchases.Add(new TicketPurchase
        {
            EventId = eventEntity.Id,
            PricingTierId = "tier1",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 50,
            TotalPrice = 500
        });

        // Act
        var availableTickets = eventEntity.AvailableTickets;

        // Assert
        availableTickets.Should().Be(0);
    }

    [Fact]
    public void Event_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(30);
        var time = new TimeSpan(19, 30, 0);
        var updatedAt = DateTime.UtcNow;

        // Act
        var eventEntity = new Event
        {
            Name = "Concert",
            Description = "Amazing concert",
            Venue = "Madison Square Garden",
            Date = date,
            Time = time,
            TotalTicketCapacity = 500,
            IsCancelled = true,
            UpdatedAt = updatedAt
        };

        // Assert
        eventEntity.Name.Should().Be("Concert");
        eventEntity.Description.Should().Be("Amazing concert");
        eventEntity.Venue.Should().Be("Madison Square Garden");
        eventEntity.Date.Should().Be(date);
        eventEntity.Time.Should().Be(time);
        eventEntity.TotalTicketCapacity.Should().Be(500);
        eventEntity.IsCancelled.Should().BeTrue();
        eventEntity.UpdatedAt.Should().Be(updatedAt);
    }
}

