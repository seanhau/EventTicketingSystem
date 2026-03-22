using System.Net;
using System.Net.Http.Json;
using Application.Reports.DTOs;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace API.IntegrationTests;

public class ReportsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ReportsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string EventId, string PricingTierId)> CreateTestEventWithPurchases()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Concert",
            Description = "A test concert",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "VIP",
            Price = 150.00m,
            Capacity = 30
        };

        var purchase = new TicketPurchase
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            PricingTierId = pricingTier.Id,
            Quantity = 5,
            TotalPrice = 750.00m,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            ConfirmationCode = "TEST123",
            PurchasedAt = DateTime.UtcNow
        };

        context.Events.Add(eventEntity);
        context.PricingTiers.Add(pricingTier);
        context.TicketPurchases.Add(purchase);
        await context.SaveChangesAsync();

        return (eventEntity.Id, pricingTier.Id);
    }

    [Fact]
    public async Task GetAllEventsSalesReport_ShouldReturnEmptyList_WhenNoEvents()
    {
        // Act
        var response = await _client.GetAsync("/api/reports/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.Content.ReadFromJsonAsync<List<EventSalesReportDto>>();
        reports.Should().NotBeNull();
        reports.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllEventsSalesReport_ShouldReturnReports_WhenEventsExist()
    {
        // Arrange
        await CreateTestEventWithPurchases();

        // Act
        var response = await _client.GetAsync("/api/reports/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.Content.ReadFromJsonAsync<List<EventSalesReportDto>>();
        reports.Should().NotBeNull();
        reports.Should().HaveCount(1);
        
        var report = reports![0];
        report.EventName.Should().Be("Test Concert");
        report.TotalTicketsSold.Should().Be(5);
        report.TotalRevenue.Should().Be(750.00m);
        report.TotalTicketsAvailable.Should().Be(95);
    }

    [Fact]
    public async Task GetAllEventsSalesReport_ShouldExcludePastEvents_WhenRequested()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pastEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Past Concert",
            Description = "A past concert",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(-10),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false
        };

        var futureEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Future Concert",
            Description = "A future concert",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false
        };

        context.Events.AddRange(pastEvent, futureEvent);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/reports/sales?includePastEvents=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.Content.ReadFromJsonAsync<List<EventSalesReportDto>>();
        reports.Should().NotBeNull();
        reports.Should().HaveCount(1);
        reports![0].EventName.Should().Be("Future Concert");
    }

    [Fact]
    public async Task GetAllEventsSalesReport_ShouldExcludeCancelledEvents_ByDefault()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cancelledEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Cancelled Concert",
            Description = "A cancelled concert",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            IsCancelled = true
        };

        var activeEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Active Concert",
            Description = "An active concert",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false
        };

        context.Events.AddRange(cancelledEvent, activeEvent);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/reports/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.Content.ReadFromJsonAsync<List<EventSalesReportDto>>();
        reports.Should().NotBeNull();
        reports.Should().HaveCount(1);
        reports![0].EventName.Should().Be("Active Concert");
    }

    [Fact]
    public async Task GetAllEventsSalesReport_ShouldIncludeCancelledEvents_WhenRequested()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cancelledEvent = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Cancelled Concert",
            Description = "A cancelled concert",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            IsCancelled = true
        };

        context.Events.Add(cancelledEvent);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/reports/sales?includeCancelled=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reports = await response.Content.ReadFromJsonAsync<List<EventSalesReportDto>>();
        reports.Should().NotBeNull();
        reports.Should().HaveCount(1);
        reports![0].EventName.Should().Be("Cancelled Concert");
    }

    [Fact]
    public async Task GetEventSalesReport_ShouldReturnReport_WhenEventExists()
    {
        // Arrange
        var (eventId, _) = await CreateTestEventWithPurchases();

        // Act
        var response = await _client.GetAsync($"/api/reports/sales/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<EventSalesReportDto>();
        report.Should().NotBeNull();
        report!.EventId.Should().Be(eventId);
        report.EventName.Should().Be("Test Concert");
        report.TotalTicketsSold.Should().Be(5);
        report.TotalRevenue.Should().Be(750.00m);
        report.TotalTicketsAvailable.Should().Be(95);
    }

    [Fact]
    public async Task GetEventSalesReport_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var nonExistentEventId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/reports/sales/{nonExistentEventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEventSalesReport_ShouldIncludePricingTierBreakdown()
    {
        // Arrange
        var (eventId, _) = await CreateTestEventWithPurchases();

        // Act
        var response = await _client.GetAsync($"/api/reports/sales/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<EventSalesReportDto>();
        report.Should().NotBeNull();
        report!.PricingTierSales.Should().HaveCount(1);
        
        var tierBreakdown = report.PricingTierSales[0];
        tierBreakdown.PricingTierName.Should().Be("VIP");
        tierBreakdown.TicketsSold.Should().Be(5);
        tierBreakdown.Revenue.Should().Be(750.00m);
    }

    [Fact]
    public async Task GetEventSalesReport_ShouldShowZeroSales_WhenNoTicketsSold()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var eventEntity = new Event
        {
            Id = Guid.NewGuid().ToString(),
            Name = "No Sales Concert",
            Description = "A concert with no sales",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            IsCancelled = false
        };

        var pricingTier = new PricingTier
        {
            Id = Guid.NewGuid().ToString(),
            EventId = eventEntity.Id,
            Name = "General",
            Price = 50.00m,
            Capacity = 100
        };

        context.Events.Add(eventEntity);
        context.PricingTiers.Add(pricingTier);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/reports/sales/{eventEntity.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<EventSalesReportDto>();
        report.Should().NotBeNull();
        report!.TotalTicketsSold.Should().Be(0);
        report.TotalRevenue.Should().Be(0);
        report.TotalTicketsAvailable.Should().Be(100);
    }
}


