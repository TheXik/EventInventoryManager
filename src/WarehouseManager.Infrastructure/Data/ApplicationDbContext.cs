using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core;
using WarehouseManager.Core.Entities;

namespace WarehouseManager.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<ItemCategory> Categories { get; set; }
}