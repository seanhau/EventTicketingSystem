using Domain;
using FluentAssertions;

namespace Application.Tests.Domain;

public class PricingTierTests
{
    [Fact]
    public void PricingTier_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var pricingTier = new PricingTier
        {
            Name = "VIP"
        };

        // Assert
        pricingTier.Id.Should().NotBeNullOrEmpty();
        pricingTier.EventId.Should().Be(string.Empty);
        pricingTier.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        pricingTier.TicketPurchases.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AvailableTickets_ShouldReturnFullCapacity_WhenNoPurchases()
    {
        // Arrange
        var pricingTier = new PricingTier
        {
            Name = "General Admission",
            Capacity = 200
        };

        // Act
        var availableTickets = pricingTier.AvailableTickets;

        // Assert
        availableTickets.Should().Be(200);
    }

    [Fact]
    public void AvailableTickets_ShouldDeductPurchasedQuantity()
    {
        // Arrange
        var pricingTier = new PricingTier
        {
            Name = "VIP",
            Capacity = 50
        };

        pricingTier.TicketPurchases.Add(new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = pricingTier.Id,
            CustomerName = "Alice",
            CustomerEmail = "alice@example.com",
            Quantity = 5,
            TotalPrice = 250
        });

        pricingTier.TicketPurchases.Add(new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = pricingTier.Id,
            CustomerName = "Bob",
            CustomerEmail = "bob@example.com",
            Quantity = 10,
            TotalPrice = 500
        });

        // Act
        var availableTickets = pricingTier.AvailableTickets;

        // Assert
        availableTickets.Should().Be(35);
    }

    [Fact]
    public void AvailableTickets_ShouldReturnZero_WhenSoldOut()
    {
        // Arrange
        var pricingTier = new PricingTier
        {
            Name = "Early Bird",
            Capacity = 25
        };

        pricingTier.TicketPurchases.Add(new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = pricingTier.Id,
            CustomerName = "Charlie",
            CustomerEmail = "charlie@example.com",
            Quantity = 25,
            TotalPrice = 500
        });

        // Act
        var availableTickets = pricingTier.AvailableTickets;

        // Assert
        availableTickets.Should().Be(0);
    }

    [Fact]
    public void PricingTier_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var eventId = Guid.NewGuid().ToString();
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var pricingTier = new PricingTier
        {
            EventId = eventId,
            Name = "Premium",
            Price = 99.99m,
            Capacity = 100,
            CreatedAt = createdAt
        };

        // Assert
        pricingTier.EventId.Should().Be(eventId);
        pricingTier.Name.Should().Be("Premium");
        pricingTier.Price.Should().Be(99.99m);
        pricingTier.Capacity.Should().Be(100);
        pricingTier.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void PricingTier_ShouldHandleMultiplePurchases()
    {
        // Arrange
        var pricingTier = new PricingTier
        {
            Name = "Standard",
            Capacity = 100
        };

        // Act
        for (int i = 0; i < 10; i++)
        {
            pricingTier.TicketPurchases.Add(new TicketPurchase
            {
                EventId = "event1",
                PricingTierId = pricingTier.Id,
                CustomerName = $"Customer {i}",
                CustomerEmail = $"customer{i}@example.com",
                Quantity = 5,
                TotalPrice = 50
            });
        }

        // Assert
        pricingTier.AvailableTickets.Should().Be(50);
        pricingTier.TicketPurchases.Should().HaveCount(10);
    }
}

