using EventTicketing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Data;

public sealed class TicketingDbContext(DbContextOptions<TicketingDbContext> options)
    : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(builder =>
        {
            builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
            builder.HasData(
                new { Id = 1, Name = "Game Dev Conference", Capacity = 100, TicketsSold = 0 },
                new { Id = 2, Name = "AI & Machine Learning Summit", Capacity = 200, TicketsSold = 0 }
            );
        });
    }
}