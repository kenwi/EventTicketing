namespace EventTicketing.Domain;

public sealed class Reservation
{
    public int Id { get; private set; }
    public int EventId { get; private set; }
    public string Code { get; private set; } = "";
    public int Quantity { get; private set; }
    public int TotalKr { get; private set; }
    public DateTimeOffset PurchasedAt { get; private set; }

    private Reservation()
    {
    }

    internal static Reservation Create(int eventId, string code, int quantity, int totalKr, DateTimeOffset purchasedAt)
        => new()
        {
            EventId = eventId,
            Code = code,
            Quantity = quantity,
            TotalKr = totalKr,
            PurchasedAt = purchasedAt
        };
}
