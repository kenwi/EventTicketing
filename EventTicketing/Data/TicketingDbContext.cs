using System.Data;
using System.Data.Common;
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
            builder.Property(r => r.HolderName).HasMaxLength(Reservation.NameMaxLength).IsRequired();
            builder.Property(r => r.RedeemedCount).IsConcurrencyToken();
            builder.HasIndex(r => r.Code).IsUnique();
        });
    }

    public void EnsureReservationColumns()
    {
        var connection = Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM pragma_table_info('Reservations')";
            using var reader = command.ExecuteReader();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (reader.Read())
            {
                names.Add(reader.GetString(0));
            }

            reader.Close();
            AddColumn(connection, names, "HolderName", "TEXT NOT NULL DEFAULT ''");
            AddColumn(connection, names, "RedeemedCount", "INTEGER NOT NULL DEFAULT 0");
        }
        finally
        {
            if (openedHere)
            {
                connection.Close();
            }
        }
    }

    private static void AddColumn(DbConnection connection, HashSet<string> names, string column, string definition)
    {
        if (names.Contains(column))
        {
            return;
        }

        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE \"Reservations\" ADD COLUMN \"{column}\" {definition}";
        alter.ExecuteNonQuery();
    }
}
