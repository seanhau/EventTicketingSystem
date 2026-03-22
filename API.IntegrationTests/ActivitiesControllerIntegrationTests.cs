using System.Net;
using System.Net.Http.Json;
using Application.Activities.DTOs;
using FluentAssertions;

namespace API.IntegrationTests;

public class ActivitiesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ActivitiesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActivities_ShouldReturnEmptyList_WhenNoActivities()
    {
        // Act
        var response = await _client.GetAsync("/api/activities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activities = await response.Content.ReadFromJsonAsync<List<BaseActivityDto>>();
        activities.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnCreated_WhenValidData()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Title = "Integration Test Activity",
            Date = DateTime.UtcNow.AddDays(15),
            Description = "A test activity for integration testing",
            Category = "Music",
            City = "New York",
            Venue = "Central Park",
            Latitude = 40.7829,
            Longitude = -73.9654
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/activities", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateActivity_ShouldReturnBadRequest_WhenInvalidData()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Title = "", // Empty title should fail validation
            Date = DateTime.UtcNow.AddDays(15),
            Description = "Description",
            Category = "Music",
            City = "New York",
            Venue = "Central Park",
            Latitude = 40.7829,
            Longitude = -73.9654
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/activities", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetActivity_ShouldReturnActivity_WhenActivityExists()
    {
        // Arrange - Create an activity first
        var createDto = new CreateActivityDto
        {
            Title = "Get Test Activity",
            Date = DateTime.UtcNow.AddDays(20),
            Description = "Activity for get test",
            Category = "Food",
            City = "San Francisco",
            Venue = "Golden Gate Park",
            Latitude = 37.7694,
            Longitude = -122.4862
        };

        var createResponse = await _client.PostAsJsonAsync("/api/activities", createDto);
        createResponse.EnsureSuccessStatusCode();
        
        // Get all activities to find the created one
        var listResponse = await _client.GetAsync("/api/activities");
        var activities = await listResponse.Content.ReadFromJsonAsync<List<BaseActivityDto>>();
        var createdActivity = activities!.FirstOrDefault(a => a.Title == "Get Test Activity");
        createdActivity.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/activities/{createdActivity!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activityDetails = await response.Content.ReadFromJsonAsync<BaseActivityDto>();
        activityDetails.Should().NotBeNull();
        activityDetails!.Title.Should().Be("Get Test Activity");
        activityDetails.Category.Should().Be("Food");
    }

    [Fact]
    public async Task GetActivity_ShouldReturnNotFound_WhenActivityDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/activities/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateActivity_ShouldReturnOk_WhenValidData()
    {
        // Arrange - Create an activity first
        var createDto = new CreateActivityDto
        {
            Title = "Original Activity",
            Date = DateTime.UtcNow.AddDays(25),
            Description = "Original description",
            Category = "Culture",
            City = "Boston",
            Venue = "Museum of Fine Arts",
            Latitude = 42.3398,
            Longitude = -71.0942
        };

        var createResponse = await _client.PostAsJsonAsync("/api/activities", createDto);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.GetAsync("/api/activities");
        var activities = await listResponse.Content.ReadFromJsonAsync<List<BaseActivityDto>>();
        var createdActivity = activities!.FirstOrDefault(a => a.Title == "Original Activity");

        var updateDto = new EditActivityDto
        {
            Id = createdActivity!.Id,
            Title = "Updated Activity",
            Date = DateTime.UtcNow.AddDays(30),
            Description = "Updated description",
            Category = "Culture",
            City = "Boston",
            Venue = "Museum of Fine Arts",
            Latitude = 42.3398,
            Longitude = -71.0942
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/activities/{createdActivity.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/activities/{createdActivity.Id}");
        var updatedActivity = await getResponse.Content.ReadFromJsonAsync<BaseActivityDto>();
        updatedActivity!.Title.Should().Be("Updated Activity");
        updatedActivity.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeleteActivity_ShouldReturnOk_WhenActivityExists()
    {
        // Arrange - Create an activity first
        var createDto = new CreateActivityDto
        {
            Title = "Activity to Delete",
            Date = DateTime.UtcNow.AddDays(10),
            Description = "This activity will be deleted",
            Category = "Travel",
            City = "Miami",
            Venue = "South Beach",
            Latitude = 25.7617,
            Longitude = -80.1918
        };

        var createResponse = await _client.PostAsJsonAsync("/api/activities", createDto);
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await _client.GetAsync("/api/activities");
        var activities = await listResponse.Content.ReadFromJsonAsync<List<BaseActivityDto>>();
        var createdActivity = activities!.FirstOrDefault(a => a.Title == "Activity to Delete");

        // Act
        var response = await _client.DeleteAsync($"/api/activities/{createdActivity!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/activities/{createdActivity.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteActivity_ShouldReturnNotFound_WhenActivityDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.DeleteAsync($"/api/activities/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("Music")]
    [InlineData("Food")]
    [InlineData("Culture")]
    [InlineData("Film")]
    [InlineData("Travel")]
    [InlineData("Drinks")]
    public async Task CreateActivity_ShouldAcceptValidCategories(string category)
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Title = $"{category} Activity",
            Date = DateTime.UtcNow.AddDays(15),
            Description = $"A {category} activity",
            Category = category,
            City = "Test City",
            Venue = "Test Venue",
            Latitude = 0,
            Longitude = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/activities", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetActivities_ShouldReturnMultipleActivities_WhenActivitiesExist()
    {
        // Arrange - Create multiple activities
        var activities = new[]
        {
            new CreateActivityDto
            {
                Title = "Activity 1",
                Date = DateTime.UtcNow.AddDays(5),
                Description = "First activity",
                Category = "Music",
                City = "City 1",
                Venue = "Venue 1",
                Latitude = 0,
                Longitude = 0
            },
            new CreateActivityDto
            {
                Title = "Activity 2",
                Date = DateTime.UtcNow.AddDays(10),
                Description = "Second activity",
                Category = "Food",
                City = "City 2",
                Venue = "Venue 2",
                Latitude = 0,
                Longitude = 0
            }
        };

        foreach (var activityDto in activities)
        {
            await _client.PostAsJsonAsync("/api/activities", activityDto);
        }

        // Act
        var response = await _client.GetAsync("/api/activities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activityList = await response.Content.ReadFromJsonAsync<List<BaseActivityDto>>();
        activityList.Should().NotBeNull();
        activityList!.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}

