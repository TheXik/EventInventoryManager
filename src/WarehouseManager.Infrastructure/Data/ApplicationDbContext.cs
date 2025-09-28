using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;
using WarehouseManager.Core.Entities.Rentals;

namespace WarehouseManager.Infrastructure.Data;

/// <summary>
/// Represents the database context for the application it is handling all data operations
/// Inherits from IdentityDbContext to include ASP.NET Identity tables
/// </summary>
/// <param name="options">The options to be used by the DbContext</param>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>
    /// Represents the collection of all inventory items in the database
    /// </summary>
    public DbSet<InventoryItem> InventoryItems { get; set; }

    /// <summary>
    /// Represents the collection of all item categories in the database
    /// </summary>
    public DbSet<ItemCategory> Categories { get; set; }

    /// <summary>
    /// Represents the collection of all events in the database
    /// </summary>
    public DbSet<Event> Events { get; set; }

    /// <summary>
    /// Represents the join table for the many-to-many relationship between Events and InventoryItems
    /// </summary>
    public DbSet<EventInventoryItem> EventInventoryItems { get; set; }

    /// <summary>
    /// Represents the collection of all rental records in the database
    /// </summary>
    public DbSet<Rental> Rentals { get; set; }

    /// <summary>
    /// Represents the collection of all items associated with rentals
    /// </summary>
    public DbSet<RentalItem> RentalItems { get; set; }

    /// <summary>
    /// Configures the database model, including entity relationships and initial data creation
    /// </summary>
    /// <param name="builder">The builder used to configure the model</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Generates the ItemCategory table with a default "Uncategorized" value
        builder.Entity<ItemCategory>().HasData(
            new ItemCategory { Id = 1, Name = "Uncategorized" });

        // Defines the composite primary key for the EventInventoryItem entity
        builder.Entity<EventInventoryItem>()
            .HasKey(ei => new { ei.EventId, ei.InventoryItemId });

        // Defines the one-to-many relationship between Event and EventInventoryItem
        builder.Entity<EventInventoryItem>()
            .HasOne(ei => ei.Event)
            .WithMany(e => e.EventInventoryItems)
            .HasForeignKey(ei => ei.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Defines the one-to-many relationship between InventoryItem and EventInventoryItem
        builder.Entity<EventInventoryItem>()
            .HasOne(ei => ei.InventoryItem)
            .WithMany(i => i.EventInventoryItems)
            .HasForeignKey(ei => ei.InventoryItemId);

        // Configures the relationship for the Rentals module
        // Defines the one-to-many relationship between Rental and RentalItem
        builder.Entity<RentalItem>()
            .HasOne(ri => ri.Rental)
            .WithMany(r => r.RentalItems)
            .HasForeignKey(ri => ri.RentalId)
            // Ensures that deleting a Rental also deletes its associated RentalItems
            .OnDelete(DeleteBehavior.Cascade);

        // Defines the relationship between RentalItem and InventoryItem
        builder.Entity<RentalItem>()
            .HasOne(ri => ri.InventoryItem)
            .WithMany()
            .HasForeignKey(ri => ri.InventoryItemId)
            // Prevents an InventoryItem from being deleted if it is part of a rental
            .OnDelete(DeleteBehavior.Restrict);
    }
}