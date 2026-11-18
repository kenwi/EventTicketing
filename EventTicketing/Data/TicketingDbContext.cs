using EventTicketing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Data;

public sealed class TicketingDbContext(DbContextOptions<TicketingDbContext> options)
    : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(builder =>
        {
            builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
            builder.Property(e => e.Venue).HasMaxLength(120).IsRequired();
            builder.Property(e => e.Hook).HasMaxLength(Event.HookMaxLength).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(Event.DescriptionMaxLength).IsRequired();
            builder.Property(e => e.Kind).HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.Version).IsConcurrencyToken();
            SeasonSeed.Apply(builder);
        });

        modelBuilder.Entity<Reservation>(builder =>
        {
            builder.Property(r => r.Code).HasMaxLength(16).IsRequired();
            builder.HasIndex(r => r.Code).IsUnique();
        });
    }
}
