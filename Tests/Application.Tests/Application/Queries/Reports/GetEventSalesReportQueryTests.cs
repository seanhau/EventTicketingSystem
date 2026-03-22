using Application.Core;
using Application.Reports.Queries;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Application.Tests.Application.Queries.Reports;

public class GetEventSalesReportQueryTests
{
    private readonly AppDbContext _context;

    public GetEventSalesReportQueryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEventNotFound()
    {
        // Arrange
        var query = new GetEventSalesReport.Query { EventId = Guid.NewGuid().ToString() };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Event not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnSalesReport_WithNoSales()
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

        var query = new GetEventSalesReport.Query { EventId = eventEntity.Id };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(eventEntity.Id, result.Value.EventId);
        Assert.Equal("Test Event", result.Value.EventName);
        Assert.Equal(0, result.Value.TotalTicketsSold);
        Assert.Equal(100, result.Value.TotalTicketsAvailable);
        Assert.Equal(0m, result.Value.TotalRevenue);
    }

    [Fact]
    public async Task Handle_ShouldCalculateSalesCorrectly()
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

        var purchase1 = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 20,
            TotalPrice = 1000.00m,
            PurchasedAt = DateTime.UtcNow
        };

        var purchase2 = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            Quantity = 15,
            TotalPrice = 750.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.AddRange(purchase1, purchase2);
        await _context.SaveChangesAsync();

        var query = new GetEventSalesReport.Query { EventId = eventEntity.Id };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(35, result.Value.TotalTicketsSold); // 20 + 15
        Assert.Equal(65, result.Value.TotalTicketsAvailable); // 100 - 35
        Assert.Equal(1750.00m, result.Value.TotalRevenue); // 1000 + 750
    }

    [Fact]
    public async Task Handle_ShouldIncludePricingTierBreakdown()
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
            Quantity = 30,
            TotalPrice = 1500.00m,
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

        var query = new GetEventSalesReport.Query { EventId = eventEntity.Id };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.PricingTierSales.Count);
        
        var generalTier = result.Value.PricingTierSales.First(pt => pt.PricingTierName == "General");
        Assert.Equal(30, generalTier.TicketsSold);
        Assert.Equal(70, generalTier.TicketsAvailable);
        Assert.Equal(1500.00m, generalTier.Revenue);
        
        var vipTier = result.Value.PricingTierSales.First(pt => pt.PricingTierName == "VIP");
        Assert.Equal(10, vipTier.TicketsSold);
        Assert.Equal(40, vipTier.TicketsAvailable);
        Assert.Equal(1000.00m, vipTier.Revenue);
    }

    [Fact]
    public async Task Handle_ShouldIncludePricingTierWithNoSales()
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

        // Only add purchase for General tier
        var purchase = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier1.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 30,
            TotalPrice = 1500.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.AddRange(pricingTier1, pricingTier2);
        _context.TicketPurchases.Add(purchase);
        await _context.SaveChangesAsync();

        var query = new GetEventSalesReport.Query { EventId = eventEntity.Id };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.PricingTierSales.Count);
        
        var vipTier = result.Value.PricingTierSales.First(pt => pt.PricingTierName == "VIP");
        Assert.Equal(0, vipTier.TicketsSold);
        Assert.Equal(50, vipTier.TicketsAvailable);
        Assert.Equal(0m, vipTier.Revenue);
    }

    [Fact]
    public async Task Handle_ShouldIncludeEventDetails()
    {
        // Arrange
        var eventDate = DateTime.UtcNow.AddDays(30);
        var eventTime = new TimeSpan(18, 30, 0);
        
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Event",
            Description = "Description",
            Venue = "Test Venue",
            Date = eventDate,
            Time = eventTime,
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

        var query = new GetEventSalesReport.Query { EventId = eventEntity.Id };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test Event", result.Value.EventName);
        Assert.Equal(eventDate, result.Value.EventDate);
    }

    [Fact]
    public async Task Handle_ShouldWorkForCancelledEvent()
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

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        var purchase = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 20,
            TotalPrice = 1000.00m,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.Add(purchase);
        await _context.SaveChangesAsync();

        var query = new GetEventSalesReport.Query { EventId = eventEntity.Id };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value.TotalTicketsSold);
        Assert.Equal(1000.00m, result.Value.TotalRevenue);
    }

    [Fact]
    public async Task Handle_ShouldWorkForPastEvent()
    {
        // Arrange
        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Past Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(-10),
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

        var purchase = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Quantity = 50,
            TotalPrice = 2500.00m,
            PurchasedAt = DateTime.UtcNow.AddDays(-15)
        };

        _context.Events.Add(eventEntity);
        _context.PricingTiers.Add(pricingTier);
        _context.TicketPurchases.Add(purchase);
        await _context.SaveChangesAsync();

        var query = new GetEventSalesReport.Query { EventId = eventEntity.Id };
        var handler = new GetEventSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(50, result.Value.TotalTicketsSold);
        Assert.Equal(2500.00m, result.Value.TotalRevenue);
    }
}


