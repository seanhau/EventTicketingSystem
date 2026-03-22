using Application.Events.Commands;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Application.Tests.Application.Commands;

public class DeleteEventCommandTests
{
    private readonly AppDbContext _context;

    public DeleteEventCommandTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldDeleteEvent_WhenNoTicketsSold()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Event to Delete",
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

        var command = new DeleteEvent.Command { Id = eventEntity.Id };
        var handler = new DeleteEvent.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var deletedEvent = await _context.Events.FindAsync(eventEntity.Id);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var command = new DeleteEvent.Command { Id = Guid.NewGuid().ToString() };
        var handler = new DeleteEvent.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Event not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTicketsPurchased()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Event with Sales",
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
            Quantity = 10,
            TotalPrice = 500.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.Add(ticketPurchase);
        await _context.SaveChangesAsync();

        var command = new DeleteEvent.Command { Id = eventEntity.Id };
        var handler = new DeleteEvent.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot delete event with existing ticket purchases", result.Error);
        
        // Verify event still exists
        var eventStillExists = await _context.Events.FindAsync(eventEntity.Id);
        Assert.NotNull(eventStillExists);
    }

    [Fact]
    public async Task Handle_ShouldDeleteEventWithPricingTiers_WhenNoTicketsSold()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Event with Pricing Tiers",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var pricingTier1 = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 50,
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

        var command = new DeleteEvent.Command { Id = eventEntity.Id };
        var handler = new DeleteEvent.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var deletedEvent = await _context.Events.FindAsync(eventEntity.Id);
        Assert.Null(deletedEvent);
        
        // Verify pricing tiers are also deleted (cascade delete)
        var pricingTiersRemaining = await _context.PricingTiers
            .Where(pt => pt.EventId == eventEntity.Id)
            .ToListAsync();
        Assert.Empty(pricingTiersRemaining);
    }

    [Fact]
    public async Task Handle_ShouldAllowDeletingCancelledEvent_WhenNoTicketsSold()
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

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var command = new DeleteEvent.Command { Id = eventEntity.Id };
        var handler = new DeleteEvent.Handler(_context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var deletedEvent = await _context.Events.FindAsync(eventEntity.Id);
        Assert.Null(deletedEvent);
    }
}


