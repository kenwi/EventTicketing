namespace EventTicketing.Domain;

public sealed class Event
{
    public const int HookMaxLength = 140;
    public const int DescriptionMaxLength = 800;

    public int Id { get; private set; }
    public string Name { get; private set; } = "";
    public EventKind Kind { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public string Venue { get; private set; } = "";
    public int PriceKr { get; private set; }
    public string Hook { get; private set; } = "";
    public string Description { get; private set; } = "";
    public int Capacity { get; private set; }
    public int TicketsSold { get; private set; }
    public int Version { get; private set; }

    public int TicketsLeft => Capacity - TicketsSold;

    private Event()
    {
    }

    public static Event Create(
        string name,
        EventKind kind,
        DateTimeOffset startsAt,
        string venue,
        int priceKr,
        string hook,
        string description,
        int capacity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(venue);
        ArgumentException.ThrowIfNullOrWhiteSpace(hook);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(priceKr);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        if (hook.Length > HookMaxLength)
        {
            throw new ArgumentException($"Hook is longer than {HookMaxLength} characters.", nameof(hook));
        }

        if (description.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Description is longer than {DescriptionMaxLength} characters.", nameof(description));
        }

        return new Event
        {
            Name = name.Trim(),
            Kind = kind,
            StartsAt = startsAt,
            Venue = venue.Trim(),
            PriceKr = priceKr,
            Hook = hook.Trim(),
            Description = description.Trim(),
            Capacity = capacity
        };
    }

    public Result<Reservation> Sell(int quantity, string code, DateTimeOffset purchasedAt)
    {
        if (quantity is < 1 or > 20)
        {
            return EventErrors.QuantityOutOfRange();
        }

        if (quantity > TicketsLeft)
        {
            return EventErrors.SoldOut(TicketsLeft, quantity);
        }

        TicketsSold += quantity;
        Version++;
        return Reservation.Create(Id, code, quantity, PriceKr * quantity, purchasedAt);
    }
}
