using Domain;
using FluentAssertions;

namespace Application.Tests.Domain;

public class TicketPurchaseTests
{
    [Fact]
    public void TicketPurchase_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var purchase = new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = "tier1",
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com"
        };

        // Assert
        purchase.Id.Should().NotBeNullOrEmpty();
        purchase.PurchasedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        purchase.ConfirmationCode.Should().BeNull();
    }

    [Fact]
    public void TicketPurchase_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var eventId = Guid.NewGuid().ToString();
        var pricingTierId = Guid.NewGuid().ToString();
        var purchasedAt = DateTime.UtcNow.AddHours(-2);
        var confirmationCode = "CONF-12345";

        // Act
        var purchase = new TicketPurchase
        {
            EventId = eventId,
            PricingTierId = pricingTierId,
            CustomerName = "Jane Smith",
            CustomerEmail = "jane.smith@example.com",
            Quantity = 3,
            TotalPrice = 299.97m,
            PurchasedAt = purchasedAt,
            ConfirmationCode = confirmationCode
        };

        // Assert
        purchase.EventId.Should().Be(eventId);
        purchase.PricingTierId.Should().Be(pricingTierId);
        purchase.CustomerName.Should().Be("Jane Smith");
        purchase.CustomerEmail.Should().Be("jane.smith@example.com");
        purchase.Quantity.Should().Be(3);
        purchase.TotalPrice.Should().Be(299.97m);
        purchase.PurchasedAt.Should().Be(purchasedAt);
        purchase.ConfirmationCode.Should().Be(confirmationCode);
    }

    [Fact]
    public void TicketPurchase_ShouldCalculateTotalPrice()
    {
        // Arrange
        var quantity = 5;
        var pricePerTicket = 49.99m;

        // Act
        var purchase = new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = "tier1",
            CustomerName = "Bob Johnson",
            CustomerEmail = "bob@example.com",
            Quantity = quantity,
            TotalPrice = quantity * pricePerTicket
        };

        // Assert
        purchase.TotalPrice.Should().Be(249.95m);
    }

    [Fact]
    public void TicketPurchase_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var purchase1 = new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = "tier1",
            CustomerName = "Customer 1",
            CustomerEmail = "customer1@example.com"
        };

        var purchase2 = new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = "tier1",
            CustomerName = "Customer 2",
            CustomerEmail = "customer2@example.com"
        };

        // Assert
        purchase1.Id.Should().NotBe(purchase2.Id);
    }

    [Theory]
    [InlineData(1, 50.00)]
    [InlineData(2, 100.00)]
    [InlineData(5, 250.00)]
    [InlineData(10, 500.00)]
    public void TicketPurchase_ShouldHandleDifferentQuantities(int quantity, decimal totalPrice)
    {
        // Arrange & Act
        var purchase = new TicketPurchase
        {
            EventId = "event1",
            PricingTierId = "tier1",
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Quantity = quantity,
            TotalPrice = totalPrice
        };

        // Assert
        purchase.Quantity.Should().Be(quantity);
        purchase.TotalPrice.Should().Be(totalPrice);
    }

    [Fact]
    public void TicketPurchase_ShouldAllowNavigationProperties()
    {
        // Arrange
        var eventEntity = new Event
        {
            Name = "Concert",
            Description = "Rock concert",
            Venue = "Arena"
        };

        var pricingTier = new PricingTier
        {
            Name = "VIP",
            Price = 150.00m,
            Capacity = 50
        };

        // Act
        var purchase = new TicketPurchase
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "Alice Wonder",
            CustomerEmail = "alice@example.com",
            Quantity = 2,
            TotalPrice = 300.00m,
            Event = eventEntity,
            PricingTier = pricingTier
        };

        // Assert
        purchase.Event.Should().NotBeNull();
        purchase.Event.Should().Be(eventEntity);
        purchase.PricingTier.Should().NotBeNull();
        purchase.PricingTier.Should().Be(pricingTier);
    }
}

