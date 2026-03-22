using Application.Events.DTOs;
using Application.Events.Queries;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Tests.Application.Queries;

public class GetEventDetailsQueryTests
{
    private readonly AppDbContext _context;

    public GetEventDetailsQueryTests()
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
            Name = "Test Concert",
            Description = "A great concert",
            Venue = "Madison Square Garden",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100
        };

        eventEntity.PricingTiers.Add(new PricingTier
        {
            EventId = eventEntity.Id,
            Name = "VIP",
            Price = 150.00m,
            Capacity = 30
        });

        eventEntity.PricingTiers.Add(new PricingTier
        {
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 70
        });

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var handler = new GetEventDetails.Handler(_context);
        var query = new GetEventDetails.Query { Id = eventEntity.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(eventEntity.Id);
        result.Value.Name.Should().Be("Test Concert");
        result.Value.PricingTiers.Should().HaveCount(2);
        result.Value.AvailableTickets.Should().Be(100);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var handler = new GetEventDetails.Handler(_context);
        var query = new GetEventDetails.Query { Id = Guid.NewGuid().ToString() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Event not found");
        result.Code.Should().Be(404);
    }

    [Fact]
    public async Task Handle_ShouldCalculateAvailableTickets_WhenTicketsPurchased()
    {
        // Arrange
        var eventEntity = new Event
        {
            Name = "Test Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 0, 0),
            TotalTicketCapacity = 100
        };

        var pricingTier = new PricingTier
        {
            EventId = eventEntity.Id,
            Name = "Standard",
            Price = 50.00m,
            Capacity = 100
        };

        eventEntity.PricingTiers.Add(pricingTier);

        var purchase = new TicketPurchase
        {
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 25,
            TotalPrice = 1250.00m
        };

        eventEntity.TicketPurchases.Add(purchase);
        pricingTier.TicketPurchases.Add(purchase);

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var handler = new GetEventDetails.Handler(_context);
        var query = new GetEventDetails.Query { Id = eventEntity.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AvailableTickets.Should().Be(75);
        result.Value.PricingTiers.First().AvailableTickets.Should().Be(75);
    }

    [Fact]
    public async Task Handle_ShouldIncludeAllPricingTierDetails()
    {
        // Arrange
        var eventEntity = new Event
        {
            Name = "Multi-Tier Event",
            Description = "Event with multiple tiers",
            Venue = "Large Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(20, 0, 0),
            TotalTicketCapacity = 300
        };

        eventEntity.PricingTiers.Add(new PricingTier
        {
            EventId = eventEntity.Id,
            Name = "Platinum",
            Price = 200.00m,
            Capacity = 50
        });

        eventEntity.PricingTiers.Add(new PricingTier
        {
            EventId = eventEntity.Id,
            Name = "Gold",
            Price = 150.00m,
            Capacity = 100
        });

        eventEntity.PricingTiers.Add(new PricingTier
        {
            EventId = eventEntity.Id,
            Name = "Silver",
            Price = 100.00m,
            Capacity = 150
        });

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var handler = new GetEventDetails.Handler(_context);
        var query = new GetEventDetails.Query { Id = eventEntity.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PricingTiers.Should().HaveCount(3);
        
        var platinumTier = result.Value.PricingTiers.First(pt => pt.Name == "Platinum");
        platinumTier.Price.Should().Be(200.00m);
        platinumTier.Capacity.Should().Be(50);
        platinumTier.AvailableTickets.Should().Be(50);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectEventMetadata()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddDays(-5);
        var updatedAt = DateTime.UtcNow.AddDays(-1);
        var eventDate = DateTime.UtcNow.AddDays(30);
        var eventTime = new TimeSpan(19, 30, 0);

        var eventEntity = new Event
        {
            Name = "Metadata Test Event",
            Description = "Testing metadata",
            Venue = "Test Venue",
            Date = eventDate,
            Time = eventTime,
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        eventEntity.PricingTiers.Add(new PricingTier
        {
            EventId = eventEntity.Id,
            Name = "Standard",
            Price = 50.00m,
            Capacity = 100
        });

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var handler = new GetEventDetails.Handler(_context);
        var query = new GetEventDetails.Query { Id = eventEntity.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Date.Should().Be(eventDate);
        result.Value.Time.Should().Be(eventTime);
        result.Value.IsCancelled.Should().BeFalse();
        result.Value.CreatedAt.Should().Be(createdAt);
        result.Value.UpdatedAt.Should().Be(updatedAt);
    }
}

