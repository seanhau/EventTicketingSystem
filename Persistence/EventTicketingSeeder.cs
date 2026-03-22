using System;
using Domain;

namespace Persistence;

public class EventTicketingSeeder
{
    public static async Task SeedData(AppDbContext context)
    {
        // Try to check if events exist, if table doesn't exist yet, proceed with seeding
        try
        {
            if (context.Events.Any()) return;
        }
        catch
        {
            // Table might not exist yet or context model not updated, proceed with seeding
        }

        var events = new List<Event>
        {
            new()
            {
                Name = "Rock Concert - The Electric Legends",
                Description = "Join us for an unforgettable night of rock music featuring The Electric Legends. Experience their greatest hits and new tracks in an electrifying live performance.",
                Venue = "Madison Square Garden, 4 Pennsylvania Plaza, New York, NY 10001, USA",
                Date = DateTime.UtcNow.AddMonths(2),
                Time = new TimeSpan(20, 0, 0), // 8:00 PM
                TotalTicketCapacity = 5000,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "General Admission", Price = 75.00m, Capacity = 3000 },
                    new() { Name = "VIP", Price = 150.00m, Capacity = 1500 },
                    new() { Name = "Platinum", Price = 300.00m, Capacity = 500 }
                }
            },
            new()
            {
                Name = "Tech Conference 2026",
                Description = "The premier technology conference featuring keynotes from industry leaders, hands-on workshops, and networking opportunities. Learn about the latest in AI, cloud computing, and software development.",
                Venue = "Moscone Center, 747 Howard St, San Francisco, CA 94103, USA",
                Date = DateTime.UtcNow.AddMonths(3),
                Time = new TimeSpan(9, 0, 0), // 9:00 AM
                TotalTicketCapacity = 2000,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "Early Bird", Price = 299.00m, Capacity = 500 },
                    new() { Name = "Standard", Price = 399.00m, Capacity = 1200 },
                    new() { Name = "Premium", Price = 599.00m, Capacity = 300 }
                }
            },
            new()
            {
                Name = "Food & Wine Festival",
                Description = "Indulge in a culinary journey featuring world-class chefs, wine tastings, cooking demonstrations, and gourmet food from around the globe.",
                Venue = "Navy Pier, 600 E Grand Ave, Chicago, IL 60611, USA",
                Date = DateTime.UtcNow.AddMonths(4),
                Time = new TimeSpan(12, 0, 0), // 12:00 PM
                TotalTicketCapacity = 3000,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "General Entry", Price = 50.00m, Capacity = 2000 },
                    new() { Name = "VIP Tasting", Price = 125.00m, Capacity = 800 },
                    new() { Name = "Chef's Table", Price = 250.00m, Capacity = 200 }
                }
            },
            new()
            {
                Name = "Jazz Night Under the Stars",
                Description = "An intimate evening of smooth jazz featuring renowned artists. Enjoy cocktails and light refreshments under the open sky.",
                Venue = "Hollywood Bowl, 2301 N Highland Ave, Los Angeles, CA 90068, USA",
                Date = DateTime.UtcNow.AddMonths(1).AddDays(15),
                Time = new TimeSpan(19, 30, 0), // 7:30 PM
                TotalTicketCapacity = 1500,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "Lawn Seating", Price = 45.00m, Capacity = 800 },
                    new() { Name = "Terrace", Price = 85.00m, Capacity = 500 },
                    new() { Name = "Box Seats", Price = 150.00m, Capacity = 200 }
                }
            },
            new()
            {
                Name = "Marathon 2026",
                Description = "Annual city marathon featuring 5K, 10K, half marathon, and full marathon distances. All fitness levels welcome!",
                Venue = "Central Park, New York, NY 10024, USA",
                Date = DateTime.UtcNow.AddMonths(5),
                Time = new TimeSpan(7, 0, 0), // 7:00 AM
                TotalTicketCapacity = 10000,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "5K Entry", Price = 35.00m, Capacity = 3000 },
                    new() { Name = "10K Entry", Price = 45.00m, Capacity = 2500 },
                    new() { Name = "Half Marathon", Price = 65.00m, Capacity = 2500 },
                    new() { Name = "Full Marathon", Price = 85.00m, Capacity = 2000 }
                }
            },
            new()
            {
                Name = "Comedy Night Spectacular",
                Description = "Laugh until you cry with top comedians from around the country. An evening of stand-up comedy you won't forget!",
                Venue = "The Comedy Store, 8433 Sunset Blvd, West Hollywood, CA 90069, USA",
                Date = DateTime.UtcNow.AddDays(45),
                Time = new TimeSpan(20, 0, 0), // 8:00 PM
                TotalTicketCapacity = 500,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "General Seating", Price = 40.00m, Capacity = 350 },
                    new() { Name = "Premium", Price = 65.00m, Capacity = 150 }
                }
            },
            new()
            {
                Name = "Art Exhibition: Modern Masters",
                Description = "Explore contemporary art from emerging and established artists. Features interactive installations, paintings, sculptures, and digital art.",
                Venue = "Museum of Modern Art, 11 W 53rd St, New York, NY 10019, USA",
                Date = DateTime.UtcNow.AddMonths(2).AddDays(10),
                Time = new TimeSpan(10, 0, 0), // 10:00 AM
                TotalTicketCapacity = 800,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "General Admission", Price = 25.00m, Capacity = 600 },
                    new() { Name = "Guided Tour", Price = 45.00m, Capacity = 200 }
                }
            },
            new()
            {
                Name = "Electronic Music Festival",
                Description = "Three-day electronic music festival featuring world-renowned DJs and producers. Multiple stages, art installations, and immersive experiences.",
                Venue = "Las Vegas Motor Speedway, 7000 N Las Vegas Blvd, Las Vegas, NV 89115, USA",
                Date = DateTime.UtcNow.AddMonths(6),
                Time = new TimeSpan(14, 0, 0), // 2:00 PM
                TotalTicketCapacity = 15000,
                PricingTiers = new List<PricingTier>
                {
                    new() { Name = "3-Day General Admission", Price = 299.00m, Capacity = 10000 },
                    new() { Name = "3-Day VIP", Price = 599.00m, Capacity = 4000 },
                    new() { Name = "3-Day Ultra VIP", Price = 1299.00m, Capacity = 1000 }
                }
            }
        };

        // Set EventId for all pricing tiers
        foreach (var evt in events)
        {
            foreach (var tier in evt.PricingTiers)
            {
                tier.EventId = evt.Id;
            }
        }

        context.Events.AddRange(events);
        await context.SaveChangesAsync();

        // Add some sample ticket purchases for demonstration
        var samplePurchases = new List<TicketPurchase>
        {
            new()
            {
                EventId = events[0].Id,
                PricingTierId = events[0].PricingTiers.First().Id,
                CustomerName = "John Doe",
                CustomerEmail = "john.doe@example.com",
                Quantity = 2,
                TotalPrice = 150.00m,
                ConfirmationCode = "ROCK2024A1",
                PurchasedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                EventId = events[0].Id,
                PricingTierId = events[0].PricingTiers.Skip(1).First().Id,
                CustomerName = "Jane Smith",
                CustomerEmail = "jane.smith@example.com",
                Quantity = 4,
                TotalPrice = 600.00m,
                ConfirmationCode = "ROCK2024B2",
                PurchasedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                EventId = events[1].Id,
                PricingTierId = events[1].PricingTiers.First().Id,
                CustomerName = "Bob Johnson",
                CustomerEmail = "bob.johnson@example.com",
                Quantity = 1,
                TotalPrice = 299.00m,
                ConfirmationCode = "TECH2026C3",
                PurchasedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                EventId = events[2].Id,
                PricingTierId = events[2].PricingTiers.Last().Id,
                CustomerName = "Alice Williams",
                CustomerEmail = "alice.williams@example.com",
                Quantity = 2,
                TotalPrice = 500.00m,
                ConfirmationCode = "FOOD2026D4",
                PurchasedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        context.TicketPurchases.AddRange(samplePurchases);
        await context.SaveChangesAsync();
    }
}


