using Application.Tickets.Queries;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Application.Tests.Application.Queries;

public class GetTicketAvailabilityQueryTests
{
    private readonly AppDbContext _context;

    public GetTicketAvailabilityQueryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnEventDetails_WhenEventExists()
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

        var query = new GetTicketAvailability.Query { EventId = eventEntity.Id };
        var handler = new GetTicketAvailability.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(eventEntity.Id, result.Value.Id);
        Assert.Equal("Test Event", result.Value.Name);
        Assert.Equal(100, result.Value.AvailableTickets);
        Assert.Single(result.Value.PricingTiers);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var query = new GetTicketAvailability.Query { EventId = Guid.NewGuid().ToString() };
        var handler = new GetTicketAvailability.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Event not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldCalculateAvailableTicketsCorrectly()
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

        var ticketPurchase = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 30,
            TotalPrice = 1500.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.Add(ticketPurchase);
        await _context.SaveChangesAsync();

        var query = new GetTicketAvailability.Query { EventId = eventEntity.Id };
        var handler = new GetTicketAvailability.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(70, result.Value!.AvailableTickets); // 100 - 30
        Assert.Equal(70, result.Value.PricingTiers[0].AvailableTickets);
    }

    [Fact]
    public async Task Handle_ShouldIncludeMultiplePricingTiers()
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
            TotalTicketCapacity = 150,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier1 = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier2 = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "VIP",
            Price = 100.00m,
            Capacity = 50,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.AddRange(pricingTier1, pricingTier2);
        await _context.SaveChangesAsync();

        var query = new GetTicketAvailability.Query { EventId = eventEntity.Id };
        var handler = new GetTicketAvailability.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.PricingTiers.Count);
        Assert.Contains(result.Value.PricingTiers, pt => pt.Name == "General" && pt.Price == 50.00m);
        Assert.Contains(result.Value.PricingTiers, pt => pt.Name == "VIP" && pt.Price == 100.00m);
    }

    [Fact]
    public async Task Handle_ShouldShowCancelledStatus()
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
            IsCancelled = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var query = new GetTicketAvailability.Query { EventId = eventEntity.Id };
        var handler = new GetTicketAvailability.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsCancelled);
    }

    [Fact]
    public async Task Handle_ShouldCalculateAvailabilityPerPricingTier()
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
            TotalTicketCapacity = 150,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier1 = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier2 = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "VIP",
            Price = 100.00m,
            Capacity = 50,
            CreatedAt = DateTime.UtcNow
        };

        var purchase1 = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier1.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 40,
            TotalPrice = 2000.00m,
            PurchasedAt = DateTime.UtcNow
        };

        var purchase2 = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier2.Id,
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            Quantity = 10,
            TotalPrice = 1000.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.AddRange(pricingTier1, pricingTier2);
        _context.TicketPurchases.AddRange(purchase1, purchase2);
        await _context.SaveChangesAsync();

        var query = new GetTicketAvailability.Query { EventId = eventEntity.Id };
        var handler = new GetTicketAvailability.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value!.AvailableTickets); // 150 - 40 - 10
        
        var generalTier = result.Value.PricingTiers.First(pt => pt.Name == "General");
        Assert.Equal(60, generalTier.AvailableTickets); // 100 - 40
        
        var vipTier = result.Value.PricingTiers.First(pt => pt.Name == "VIP");
        Assert.Equal(40, vipTier.AvailableTickets); // 50 - 10
    }
}


