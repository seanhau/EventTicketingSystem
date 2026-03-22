using Application.Core;
using Application.Events.Commands;
using Application.Events.DTOs;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Application.Tests.Application.Commands;

public class CreateEventCommandTests
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CreateEventCommandTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options)
        {
            Events = null!,
            PricingTiers = null!,
            TicketPurchases = null!
        };

        var mapperConfig = new MapperConfiguration(cfg => {
            cfg.CreateMap<CreateEventDto, Event>()
                .ForMember(dest => dest.PricingTiers, opt => opt.Ignore());
        });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_ShouldCreateEvent_WhenValidDataProvided()
    {
        // Arrange
        var handler = new CreateEvent.Handler(_context, _mapper);
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Test Concert",
                Description = "A great concert",
                Venue = "Madison Square Garden",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 30, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "VIP", Price = 150.00m, Capacity = 30 },
                    new() { Name = "General", Price = 50.00m, Capacity = 70 }
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();

        var createdEvent = await _context.Events
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == result.Value);

        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be("Test Concert");
        createdEvent.PricingTiers.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCapacityMismatch()
    {
        // Arrange
        var handler = new CreateEvent.Handler(_context, _mapper);
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Test Event",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "VIP", Price = 150.00m, Capacity = 30 },
                    new() { Name = "General", Price = 50.00m, Capacity = 50 } // Total = 80, not 100
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Total ticket capacity must equal the sum of all pricing tier capacities");
        result.Code.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDuplicatePricingTierNames()
    {
        // Arrange
        var handler = new CreateEvent.Handler(_context, _mapper);
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Test Event",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "VIP", Price = 150.00m, Capacity = 50 },
                    new() { Name = "vip", Price = 100.00m, Capacity = 50 } // Duplicate (case-insensitive)
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Duplicate pricing tier names found");
        result.Code.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ShouldCreatePricingTiersWithCorrectEventId()
    {
        // Arrange
        var handler = new CreateEvent.Handler(_context, _mapper);
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Test Event",
                Description = "Description",
                Venue = "Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(19, 0, 0),
                TotalTicketCapacity = 100,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Standard", Price = 50.00m, Capacity = 100 }
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var createdEvent = await _context.Events
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == result.Value);

        createdEvent!.PricingTiers.Should().HaveCount(1);
        createdEvent.PricingTiers.First().EventId.Should().Be(createdEvent.Id);
    }

    [Fact]
    public async Task Handle_ShouldHandleMultiplePricingTiers()
    {
        // Arrange
        var handler = new CreateEvent.Handler(_context, _mapper);
        var command = new CreateEvent.Command
        {
            EventDto = new CreateEventDto
            {
                Name = "Multi-Tier Event",
                Description = "Event with multiple tiers",
                Venue = "Large Venue",
                Date = DateTime.UtcNow.AddDays(30),
                Time = new TimeSpan(20, 0, 0),
                TotalTicketCapacity = 500,
                PricingTiers = new List<PricingTierDto>
                {
                    new() { Name = "Platinum", Price = 200.00m, Capacity = 50 },
                    new() { Name = "Gold", Price = 150.00m, Capacity = 100 },
                    new() { Name = "Silver", Price = 100.00m, Capacity = 150 },
                    new() { Name = "Bronze", Price = 50.00m, Capacity = 200 }
                }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var createdEvent = await _context.Events
            .Include(e => e.PricingTiers)
            .FirstOrDefaultAsync(e => e.Id == result.Value);

        createdEvent!.PricingTiers.Should().HaveCount(4);
        createdEvent.PricingTiers.Select(pt => pt.Name).Should().Contain(new[] { "Platinum", "Gold", "Silver", "Bronze" });
    }
}

