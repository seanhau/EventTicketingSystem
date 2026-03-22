using Application.Reports.Queries;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Application.Tests.Application.Queries.Reports;

public class GetAllEventsSalesReportQueryTests
{
    private readonly AppDbContext _context;

    public GetAllEventsSalesReportQueryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoEvents()
    {
        // Arrange
        var query = new GetAllEventsSalesReport.Query();
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
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

        var query = new GetAllEventsSalesReport.Query();
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0].TotalTicketsSold);
        Assert.Equal(100, result[0].TotalTicketsAvailable);
        Assert.Equal(0m, result[0].TotalRevenue);
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

        var query = new GetAllEventsSalesReport.Query();
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(35, result[0].TotalTicketsSold); // 20 + 15
        Assert.Equal(65, result[0].TotalTicketsAvailable); // 100 - 35
        Assert.Equal(1750.00m, result[0].TotalRevenue); // 1000 + 750
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

        var query = new GetAllEventsSalesReport.Query();
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result[0].PricingTierSales.Count);
        
        var generalTier = result[0].PricingTierSales.First(pt => pt.PricingTierName == "General");
        Assert.Equal(30, generalTier.TicketsSold);
        Assert.Equal(70, generalTier.TicketsAvailable);
        Assert.Equal(1500.00m, generalTier.Revenue);
        
        var vipTier = result[0].PricingTierSales.First(pt => pt.PricingTierName == "VIP");
        Assert.Equal(10, vipTier.TicketsSold);
        Assert.Equal(40, vipTier.TicketsAvailable);
        Assert.Equal(1000.00m, vipTier.Revenue);
    }

    [Fact]
    public async Task Handle_ShouldExcludePastEvents_WhenRequested()
    {
        // Arrange
        var pastEvent = new Event
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

        var futureEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Future Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(10),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.AddRange(pastEvent, futureEvent);
        await _context.SaveChangesAsync();

        var query = new GetAllEventsSalesReport.Query { IncludePastEvents = false };
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Future Event", result[0].EventName);
    }

    [Fact]
    public async Task Handle_ShouldIncludePastEvents_ByDefault()
    {
        // Arrange
        var pastEvent = new Event
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

        var futureEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Future Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(10),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.AddRange(pastEvent, futureEvent);
        await _context.SaveChangesAsync();

        var query = new GetAllEventsSalesReport.Query { IncludePastEvents = true };
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_ShouldExcludeCancelledEvents_ByDefault()
    {
        // Arrange
        var activeEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Active Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(10),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var cancelledEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Cancelled Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(10),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.AddRange(activeEvent, cancelledEvent);
        await _context.SaveChangesAsync();

        var query = new GetAllEventsSalesReport.Query { IncludeCancelled = false };
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Event", result[0].EventName);
    }

    [Fact]
    public async Task Handle_ShouldIncludeCancelledEvents_WhenRequested()
    {
        // Arrange
        var activeEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Active Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(10),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var cancelledEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Cancelled Event",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(10),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.AddRange(activeEvent, cancelledEvent);
        await _context.SaveChangesAsync();

        var query = new GetAllEventsSalesReport.Query { IncludeCancelled = true };
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_ShouldOrderEventsByDate()
    {
        // Arrange
        var event1 = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Event 1",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(20),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        var event2 = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Event 2",
            Description = "Description",
            Venue = "Venue",
            Date = DateTime.UtcNow.AddDays(10),
            Time = new TimeSpan(18, 0, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.AddRange(event1, event2);
        await _context.SaveChangesAsync();

        var query = new GetAllEventsSalesReport.Query();
        var handler = new GetAllEventsSalesReport.Handler(_context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Event 2", result[0].EventName); // Earlier date first
        Assert.Equal("Event 1", result[1].EventName);
    }
}


