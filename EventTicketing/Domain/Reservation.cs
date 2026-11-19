namespace EventTicketing.Domain;

public sealed class Reservation
{
    public const int NameMaxLength = 80;

    public int Id { get; private set; }
    public int EventId { get; private set; }
    public string Code { get; private set; } = "";
    public string HolderName { get; private set; } = "";
    public int Quantity { get; private set; }
    public int RedeemedCount { get; private set; }
    public int TotalKr { get; private set; }
    public DateTimeOffset PurchasedAt { get; private set; }

    public int EntriesLeft => Quantity - RedeemedCount;

    private Reservation()
    {
    }

    internal static Reservation Create(
        int eventId,
        string code,
        string holderName,
        int quantity,
        int totalKr,
        DateTimeOffset purchasedAt)
        => new()
        {
            EventId = eventId,
            Code = code,
            HolderName = holderName,
            Quantity = quantity,
            TotalKr = totalKr,
            PurchasedAt = purchasedAt
        };

    public Result Redeem()
    {
        if (RedeemedCount >= Quantity)
        {
            return EventErrors.FullyRedeemed(Quantity);
        }

        RedeemedCount++;
        return Result.Success();
    }
}
