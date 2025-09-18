using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core;
using WarehouseManager.Core.Entities;
using WarehouseManager.Core.Entities.InventoryPage;

namespace WarehouseManager.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<ItemCategory> Categories { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventInventoryItem> EventInventoryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ItemCategory>().HasData(
            new ItemCategory { Id = 1, Name = "Uncategorized" });
        
        builder.Entity<EventInventoryItem>()
            .HasKey(ei => new { ei.EventId, ei.InventoryItemId });

        builder.Entity<EventInventoryItem>()
            .HasOne(ei => ei.Event)
            .WithMany(e => e.EventInventoryItems)
            .HasForeignKey(ei => ei.EventId);

        builder.Entity<EventInventoryItem>()
            .HasOne(ei => ei.InventoryItem)
            .WithMany(i => i.EventInventoryItems)
            .HasForeignKey(ei => ei.InventoryItemId);
    }
}