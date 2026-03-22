using Domain;
using FluentAssertions;

namespace Application.Tests.Domain;

public class ActivityTests
{
    [Fact]
    public void Activity_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var activity = new Activity
        {
            Title = "Test Activity",
            Description = "Test Description",
            Category = "Music",
            City = "New York",
            Venue = "Central Park"
        };

        // Assert
        activity.Id.Should().NotBeNullOrEmpty();
        activity.IsCancelled.Should().BeFalse();
        activity.Latitude.Should().Be(0);
        activity.Longitude.Should().Be(0);
    }

    [Fact]
    public void Activity_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(7);
        var latitude = 40.7128;
        var longitude = -74.0060;

        // Act
        var activity = new Activity
        {
            Title = "Jazz Concert",
            Date = date,
            Description = "Live jazz performance",
            Category = "Music",
            IsCancelled = false,
            City = "New York",
            Venue = "Blue Note",
            Latitude = latitude,
            Longitude = longitude
        };

        // Assert
        activity.Title.Should().Be("Jazz Concert");
        activity.Date.Should().Be(date);
        activity.Description.Should().Be("Live jazz performance");
        activity.Category.Should().Be("Music");
        activity.IsCancelled.Should().BeFalse();
        activity.City.Should().Be("New York");
        activity.Venue.Should().Be("Blue Note");
        activity.Latitude.Should().Be(latitude);
        activity.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Activity_ShouldAllowCancellation()
    {
        // Arrange
        var activity = new Activity
        {
            Title = "Workshop",
            Description = "Coding workshop",
            Category = "Education",
            City = "San Francisco",
            Venue = "Tech Hub"
        };

        // Act
        activity.IsCancelled = true;

        // Assert
        activity.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Activity_ShouldHandleCoordinates()
    {
        // Arrange & Act
        var activity = new Activity
        {
            Title = "Beach Party",
            Description = "Summer beach party",
            Category = "Social",
            City = "Miami",
            Venue = "South Beach",
            Latitude = 25.7617,
            Longitude = -80.1918
        };

        // Assert
        activity.Latitude.Should().BeApproximately(25.7617, 0.0001);
        activity.Longitude.Should().BeApproximately(-80.1918, 0.0001);
    }

    [Theory]
    [InlineData("Music")]
    [InlineData("Food")]
    [InlineData("Culture")]
    [InlineData("Film")]
    [InlineData("Travel")]
    [InlineData("Drinks")]
    public void Activity_ShouldAcceptValidCategories(string category)
    {
        // Arrange & Act
        var activity = new Activity
        {
            Title = "Test Activity",
            Description = "Test Description",
            Category = category,
            City = "Test City",
            Venue = "Test Venue"
        };

        // Assert
        activity.Category.Should().Be(category);
    }

    [Fact]
    public void Activity_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var activity1 = new Activity
        {
            Title = "Activity 1",
            Description = "Description 1",
            Category = "Music",
            City = "City 1",
            Venue = "Venue 1"
        };

        var activity2 = new Activity
        {
            Title = "Activity 2",
            Description = "Description 2",
            Category = "Food",
            City = "City 2",
            Venue = "Venue 2"
        };

        // Assert
        activity1.Id.Should().NotBe(activity2.Id);
    }
}

