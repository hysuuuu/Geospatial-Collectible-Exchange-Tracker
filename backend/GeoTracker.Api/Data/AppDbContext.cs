using GeoTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoTracker.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Collectible> Collectibles { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
        public DbSet<ExchangeRequest> ExchangeRequests { get; set; }
        
        // Composite Unique Index
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInventory>()
                .HasIndex(ui => new { ui.UserId, ui.CollectibleId })
                .IsUnique();
        }
    }
}