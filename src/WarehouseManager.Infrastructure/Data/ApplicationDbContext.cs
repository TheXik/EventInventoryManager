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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ItemCategory>().HasData(
            new ItemCategory { Id = 1, Name = "Uncategorized" });
    }
}