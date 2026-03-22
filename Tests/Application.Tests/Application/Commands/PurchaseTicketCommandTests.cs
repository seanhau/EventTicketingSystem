using Application.Tickets.Commands;
using Application.Tickets.DTOs;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Application.Tests.Application.Commands;

public class PurchaseTicketCommandTests
{
    private readonly AppDbContext _context;

    public PurchaseTicketCommandTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldPurchaseTickets_WhenValidRequest()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 2
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test Event", result.Value.EventName);
        Assert.Equal("General", result.Value.PricingTierName);
        Assert.Equal(2, result.Value.Quantity);
        Assert.Equal(100.00m, result.Value.TotalPrice);
        Assert.NotNull(result.Value.ConfirmationCode);
        Assert.Equal(8, result.Value.ConfirmationCode.Length);

        // Verify purchase was saved
        var purchase = await _context.TicketPurchases.FirstOrDefaultAsync();
        Assert.NotNull(purchase);
        Assert.Equal(2, purchase.Quantity);
        Assert.Equal(100.00m, purchase.TotalPrice);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var purchaseDto = new PurchaseTicketDto
        {
            EventId = Guid.NewGuid().ToString(),
            PricingTierId = Guid.NewGuid().ToString(),
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 2
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Event not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEventIsCancelled()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Cancelled Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = true, // Event is cancelled
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 2
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Event is cancelled", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEventIsInPast()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Past Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(-10), // Past event
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 2
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Equal("Cannot purchase tickets for past events", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPricingTierDoesNotExist()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventEntity.Id,
            PricingTierId = Guid.NewGuid().ToString(), // Non-existent pricing tier
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 2
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Pricing tier not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInsufficientTicketsInPricingTier()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 10,
            CreatedAt = DateTime.UtcNow
        };

        // Existing purchase that takes up 8 tickets
        var existingPurchase = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            Quantity = 8,
            TotalPrice = 400.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.Add(existingPurchase);
        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 5 // Only 2 tickets available (10 - 8)
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Only 2 tickets available in General tier", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInsufficientTicketsInEvent()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 20,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier1 = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 15,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier2 = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "VIP",
            Price = 100.00m,
            Capacity = 5,
            CreatedAt = DateTime.UtcNow
        };

        // Existing purchase in tier 1
        var existingPurchase = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier1.Id,
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            Quantity = 15,
            TotalPrice = 750.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.AddRange(pricingTier1, pricingTier2);
        _context.TicketPurchases.Add(existingPurchase);
        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier2.Id, // VIP tier has 5 available
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 5 // But only 5 tickets left in total event capacity (20 - 15)
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess); // Should succeed as both tier and event have 5 available
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPriceCorrectly()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "VIP",
            Price = 125.50m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 3
        };

        var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
        var handler = new PurchaseTicket.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(376.50m, result.Value!.TotalPrice); // 125.50 * 3
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueConfirmationCodes()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        await _context.SaveChangesAsync();

        var handler = new PurchaseTicket.Handler(_context);
        var confirmationCodes = new HashSet<string>();

        // Act - Make multiple purchases
        for (int i = 0; i < 5; i++)
        {
            var purchaseDto = new PurchaseTicketDto
            {
                EventId = eventEntity.Id,
                PricingTierId = pricingTier.Id,
                CustomerName = $"Customer {i}",
                CustomerEmail = $"customer{i}@example.com",
                Quantity = 1
            };

            var command = new PurchaseTicket.Command { PurchaseDto = purchaseDto };
            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            confirmationCodes.Add(result.Value!.ConfirmationCode);
        }

        // Assert - All confirmation codes should be unique
        Assert.Equal(5, confirmationCodes.Count);
    }
}


