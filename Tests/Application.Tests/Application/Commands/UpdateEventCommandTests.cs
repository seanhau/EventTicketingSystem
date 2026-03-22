using Application.Core;
using Application.Events.Commands;
using Application.Events.DTOs;
using AutoMapper;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Application.Tests.Application.Commands;

public class UpdateEventCommandTests
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public UpdateEventCommandTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Create mapper with MappingProfiles
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfiles());
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ShouldUpdateEvent_WhenValidDataProvided()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Original Event",
            Description = "Original Description",
            Venue = "Original Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateEventDto
        {
            Id = eventEntity.Id,
            Name = "Updated Event",
            Description = "Updated Description",
            Venue = "Updated Venue",
            Date = DateTime.UtcNow.AddDays(45),
            Time = new TimeSpan(20, 0, 0),
            TotalTicketCapacity = 150,
            IsCancelled = false
        };

        var command = new UpdateEvent.Command { EventDto = updateDto };
        var handler = new UpdateEvent.Handler(_context, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedEvent = await _context.Events.FindAsync(eventEntity.Id);
        Assert.NotNull(updatedEvent);
        Assert.Equal("Updated Event", updatedEvent.Name);
        Assert.Equal("Updated Description", updatedEvent.Description);
        Assert.Equal("Updated Venue", updatedEvent.Venue);
        Assert.Equal(150, updatedEvent.TotalTicketCapacity);
        Assert.NotNull(updatedEvent.UpdatedAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateEventDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Non-existent Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false
        };

        var command = new UpdateEvent.Command { EventDto = updateDto };
        var handler = new UpdateEvent.Handler(_context, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("Event not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenReducingCapacityBelowTicketsSold()
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
            Quantity = 50,
            TotalPrice = 2500.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.Add(ticketPurchase);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateEventDto
        {
            Id = eventEntity.Id,
            Name = "Updated Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 40, // Less than 50 tickets sold
            IsCancelled = false
        };

        var command = new UpdateEvent.Command { EventDto = updateDto };
        var handler = new UpdateEvent.Handler(_context, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.Code);
        Assert.Contains("Cannot reduce capacity below 50 tickets already sold", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldAllowIncreasingCapacity_WhenTicketsSold()
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
            Quantity = 50,
            TotalPrice = 2500.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.Add(ticketPurchase);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateEventDto
        {
            Id = eventEntity.Id,
            Name = "Updated Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 200, // Increasing capacity
            IsCancelled = false
        };

        var command = new UpdateEvent.Command { EventDto = updateDto };
        var handler = new UpdateEvent.Handler(_context, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedEvent = await _context.Events.FindAsync(eventEntity.Id);
        Assert.Equal(200, updatedEvent!.TotalTicketCapacity);
    }

    [Fact]
    public async Task Handle_ShouldAllowCancellingEvent()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Event to Cancel",
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

        var updateDto = new UpdateEventDto
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            Venue = eventEntity.Venue,
            Date = eventEntity.Date,
            Time = eventEntity.Time,
            TotalTicketCapacity = eventEntity.TotalTicketCapacity,
            IsCancelled = true // Cancelling the event
        };

        var command = new UpdateEvent.Command { EventDto = updateDto };
        var handler = new UpdateEvent.Handler(_context, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedEvent = await _context.Events.FindAsync(eventEntity.Id);
        Assert.True(updatedEvent!.IsCancelled);
    }
}


