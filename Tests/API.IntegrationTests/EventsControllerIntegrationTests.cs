using System.Net;
using System.Net.Http.Json;
using Application.Events.DTOs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace API.IntegrationTests;

public class EventsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public EventsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEvents_ShouldReturnEmptyList_WhenNoEvents()
    {
        // Act
        var response = await _client.GetAsync("/api/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<EventDetailsDto>>();
        events.Should().NotBeNull();
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateEvent_ShouldReturnCreated_WhenValidData()
    {
        // Arrange
        var createDto = new CreateEventDto
        {
            Name = "Integration Test Concert",
            Description = "A test concert for integration testing",
            Venue = "Test Arena",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 30, 0),
            TotalTicketCapacity = 100,
            PricingTiers = new List<PricingTierDto>
            {
                new() { Name = "VIP", Price = 150.00m, Capacity = 30 },
                new() { Name = "General", Price = 50.00m, Capacity = 70 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var eventId = await response.Content.ReadAsStringAsync();
        eventId.Should().NotBeNullOrEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateEvent_ShouldReturnBadRequest_WhenCapacityMismatch()
    {
        // Arrange
        var createDto = new CreateEventDto
        {
            Name = "Invalid Event",
            Description = "Event with capacity mismatch",
            Venue = "Test Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 0, 0),
            TotalTicketCapacity = 100,
            PricingTiers = new List<PricingTierDto>
            {
                new() { Name = "VIP", Price = 150.00m, Capacity = 30 },
                new() { Name = "General", Price = 50.00m, Capacity = 50 } // Total = 80, not 100
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEvent_ShouldReturnBadRequest_WhenDuplicatePricingTiers()
    {
        // Arrange
        var createDto = new CreateEventDto
        {
            Name = "Duplicate Tier Event",
            Description = "Event with duplicate pricing tiers",
            Venue = "Test Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 0, 0),
            TotalTicketCapacity = 100,
            PricingTiers = new List<PricingTierDto>
            {
                new() { Name = "VIP", Price = 150.00m, Capacity = 50 },
                new() { Name = "vip", Price = 100.00m, Capacity = 50 } // Duplicate (case-insensitive)
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/events", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEvent_ShouldReturnEvent_WhenEventExists()
    {
        // Arrange - Create an event first
        var createDto = new CreateEventDto
        {
            Name = "Get Test Event",
            Description = "Event for get test",
            Venue = "Test Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(20, 0, 0),
            TotalTicketCapacity = 200,
            PricingTiers = new List<PricingTierDto>
            {
                new() { Name = "Standard", Price = 75.00m, Capacity = 200 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/events", createDto);
        var eventId = await createResponse.Content.ReadAsStringAsync();
        eventId = eventId.Trim('"'); // Remove quotes from JSON string

        // Act
        var response = await _client.GetAsync($"/api/events/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var eventDetails = await response.Content.ReadFromJsonAsync<EventDetailsDto>();
        eventDetails.Should().NotBeNull();
        eventDetails!.Id.Should().Be(eventId);
        eventDetails.Name.Should().Be("Get Test Event");
        eventDetails.PricingTiers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/events/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateEvent_ShouldReturnNoContent_WhenValidData()
    {
        // Arrange - Create an event first
        var createDto = new CreateEventDto
        {
            Name = "Original Event",
            Description = "Original description",
            Venue = "Original Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 0, 0),
            TotalTicketCapacity = 100,
            PricingTiers = new List<PricingTierDto>
            {
                new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/events", createDto);
        var eventId = await createResponse.Content.ReadAsStringAsync();
        eventId = eventId.Trim('"');

        var updateDto = new UpdateEventDto
        {
            Id = eventId,
            Name = "Updated Event",
            Description = "Updated description",
            Venue = "Updated Venue",
            Date = DateTime.UtcNow.AddDays(45),
            Time = new TimeSpan(20, 0, 0),
            TotalTicketCapacity = 100
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/events/{eventId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/events/{eventId}");
        var updatedEvent = await getResponse.Content.ReadFromJsonAsync<EventDetailsDto>();
        updatedEvent!.Name.Should().Be("Updated Event");
        updatedEvent.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateEvent_ShouldReturnBadRequest_WhenIdMismatch()
    {
        // Arrange
        var eventId = Guid.NewGuid().ToString();
        var differentId = Guid.NewGuid().ToString();

        var updateDto = new UpdateEventDto
        {
            Id = differentId,
            Name = "Updated Event",
            Description = "Updated description",
            Venue = "Updated Venue",
            Date = DateTime.UtcNow.AddDays(45),
            Time = new TimeSpan(20, 0, 0),
            TotalTicketCapacity = 100
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/events/{eventId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteEvent_ShouldReturnNoContent_WhenEventExists()
    {
        // Arrange - Create an event first
        var createDto = new CreateEventDto
        {
            Name = "Event to Delete",
            Description = "This event will be deleted",
            Venue = "Delete Venue",
            Date = DateTime.UtcNow.AddDays(30),
            Time = new TimeSpan(19, 0, 0),
            TotalTicketCapacity = 100,
            PricingTiers = new List<PricingTierDto>
            {
                new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/events", createDto);
        var eventId = await createResponse.Content.ReadAsStringAsync();
        eventId = eventId.Trim('"');

        // Act
        var response = await _client.DeleteAsync($"/api/events/{eventId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/events/{eventId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.DeleteAsync($"/api/events/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnMultipleEvents_WhenEventsExist()
    {
        // Arrange - Create multiple events
        var events = new[]
        {
            new CreateEventDto
            {
                Name = "Event 1",
                Description = "First event",
                Venue = "Venue 1",
                Date = DateTime.UtcNow.AddDays(10),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
                }
            },
            new CreateEventDto
            {
                Name = "Event 2",
                Description = "Second event",
                Venue = "Venue 2",
                Date = DateTime.UtcNow.AddDays(20),
                Time = new TimeSpan(20, 0, 0),
                TotalTicketCapacity = 150,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 60.00m, Capacity = 150 }
                }
            }
        };

        foreach (var eventDto in events)
        {
            await _client.PostAsJsonAsync("/api/events", eventDto);
        }

        // Act
        var response = await _client.GetAsync("/api/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var eventList = await response.Content.ReadFromJsonAsync<List<EventDetailsDto>>();
        eventList.Should().NotBeNull();
        eventList!.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}

