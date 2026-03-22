using System.Net;
using System.Net.Http.Json;
using Application.Events.DTOs;
using Application.Tickets.DTOs;
using Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace API.IntegrationTests;

public class TicketsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TicketsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> CreateTestEvent()
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

        var pricingTiers = new List<PricingTier>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                EventId = eventEntity.Id,
                Name = "VIP",
                Price = 150.00m,
                Capacity = 30
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                EventId = eventEntity.Id,
                Name = "General",
                Price = 50.00m,
                Capacity = 70
            }
        };

        context.Events.Add(eventEntity);
        context.PricingTiers.AddRange(pricingTiers);
        await context.SaveChangesAsync();

        return eventEntity.Id;
    }

    [Fact]
    public async Task PurchaseTicket_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var eventId = await CreateTestEvent();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pricingTier = context.PricingTiers.First(pt => pt.EventId == eventId && pt.Name == "VIP");

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventId,
            PricingTierId = pricingTier.Id,
            Quantity = 2,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets/purchase", purchaseDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TicketPurchaseResponseDto>();
        result.Should().NotBeNull();
        result!.ConfirmationCode.Should().NotBeNullOrEmpty();
        result.Quantity.Should().Be(2);
        result.TotalPrice.Should().Be(300.00m); // 2 * 150
        result.EventName.Should().Be("Test Concert");
        result.PricingTierName.Should().Be("VIP");
    }

    [Fact]
    public async Task PurchaseTicket_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var purchaseDto = new PurchaseTicketDto
        {
            EventId = Guid.NewGuid().ToString(),
            PricingTierId = Guid.NewGuid().ToString(),
            Quantity = 1,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets/purchase", purchaseDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PurchaseTicket_ShouldReturnBadRequest_WhenInsufficientTickets()
    {
        // Arrange
        var eventId = await CreateTestEvent();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pricingTier = context.PricingTiers.First(pt => pt.EventId == eventId && pt.Name == "VIP");

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventId,
            PricingTierId = pricingTier.Id,
            Quantity = 50, // More than VIP capacity of 30
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets/purchase", purchaseDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PurchaseTicket_ShouldReturnBadRequest_WhenInvalidEmail()
    {
        // Arrange
        var eventId = await CreateTestEvent();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pricingTier = context.PricingTiers.First(pt => pt.EventId == eventId);

        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventId,
            PricingTierId = pricingTier.Id,
            Quantity = 1,
            CustomerName = "John Doe",
            CustomerEmail = "invalid-email" // Invalid email format
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tickets/purchase", purchaseDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTicketAvailability_ShouldReturnEventDetails_WhenEventExists()
    {
        // Arrange
        var eventId = await CreateTestEvent();

        // Act
        var response = await _client.GetAsync($"/api/tickets/availability/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EventDetailsDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(eventId);
        result.Name.Should().Be("Test Concert");
        result.PricingTiers.Should().HaveCount(2);
        result.AvailableTickets.Should().Be(100);
    }

    [Fact]
    public async Task GetTicketAvailability_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var nonExistentEventId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/tickets/availability/{nonExistentEventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTicketAvailability_ShouldReflectPurchases_WhenTicketsSold()
    {
        // Arrange
        var eventId = await CreateTestEvent();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pricingTier = context.PricingTiers.First(pt => pt.EventId == eventId && pt.Name == "VIP");

        // Purchase some tickets first
        var purchaseDto = new PurchaseTicketDto
        {
            EventId = eventId,
            PricingTierId = pricingTier.Id,
            Quantity = 5,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com"
        };
        await _client.PostAsJsonAsync("/api/tickets/purchase", purchaseDto);

        // Act
        var response = await _client.GetAsync($"/api/tickets/availability/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EventDetailsDto>();
        result.Should().NotBeNull();
        result!.AvailableTickets.Should().Be(95); // 100 - 5
        
        var vipTier = result.PricingTiers.First(pt => pt.Name == "VIP");
        vipTier.AvailableTickets.Should().Be(25); // 30 - 5
    }

    [Fact]
    public async Task PurchaseTicket_ShouldGenerateUniqueConfirmationCodes()
    {
        // Arrange
        var eventId = await CreateTestEvent();
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pricingTier = context.PricingTiers.First(pt => pt.EventId == eventId);

        var purchaseDto1 = new PurchaseTicketDto
        {
            EventId = eventId,
            PricingTierId = pricingTier.Id,
            Quantity = 1,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com"
        };

        var purchaseDto2 = new PurchaseTicketDto
        {
            EventId = eventId,
            PricingTierId = pricingTier.Id,
            Quantity = 1,
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com"
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/tickets/purchase", purchaseDto1);
        var response2 = await _client.PostAsJsonAsync("/api/tickets/purchase", purchaseDto2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var result1 = await response1.Content.ReadFromJsonAsync<TicketPurchaseResponseDto>();
        var result2 = await response2.Content.ReadFromJsonAsync<TicketPurchaseResponseDto>();

        result1!.ConfirmationCode.Should().NotBe(result2!.ConfirmationCode);
    }
}


