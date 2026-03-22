using System;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<PricingTier> PricingTiers { get; set; } = null!;
    public DbSet<TicketPurchase> TicketPurchases { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Venue).IsRequired().HasMaxLength(300);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.Date, e.IsCancelled });
        });

        // PricingTier configuration
        modelBuilder.Entity<PricingTier>(entity =>
        {
            entity.HasKey(pt => pt.Id);
            entity.Property(pt => pt.Name).IsRequired().HasMaxLength(100);
            entity.Property(pt => pt.Price).HasPrecision(18, 2);
            
            entity.HasOne(pt => pt.Event)
                .WithMany(e => e.PricingTiers)
                .HasForeignKey(pt => pt.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(pt => pt.EventId);
        });

        // TicketPurchase configuration
        modelBuilder.Entity<TicketPurchase>(entity =>
        {
            entity.HasKey(tp => tp.Id);
            entity.Property(tp => tp.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(tp => tp.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(tp => tp.TotalPrice).HasPrecision(18, 2);
            entity.Property(tp => tp.ConfirmationCode).HasMaxLength(50);

            entity.HasOne(tp => tp.Event)
                .WithMany(e => e.TicketPurchases)
                .HasForeignKey(tp => tp.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(tp => tp.PricingTier)
                .WithMany(pt => pt.TicketPurchases)
                .HasForeignKey(tp => tp.PricingTierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(tp => tp.EventId);
            entity.HasIndex(tp => tp.PricingTierId);
            entity.HasIndex(tp => tp.CustomerEmail);
            entity.HasIndex(tp => tp.ConfirmationCode);
        });
    }
}