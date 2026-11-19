namespace EventTicketing.Domain;

public static class EventErrors
{
    public static Error NotFound(int eventId)
        => new("Event.NotFound", "That event is not on the calendar.", ErrorType.NotFound);

    public static Error TicketNotFound(string code)
        => new("Ticket.NotFound", "No ticket with that code.", ErrorType.NotFound);

    public static Error SoldOut(int ticketsLeft, int requested)
        => new("Event.SoldOut", $"There are only {ticketsLeft} left. You asked for {requested}.", ErrorType.Conflict);

    public static Error QuantityOutOfRange()
        => new("Ticket.Quantity", "Pick between 1 and 20.", ErrorType.Validation);

    public static Error NameInvalid()
        => new("Ticket.Name", $"Enter a name, up to {Reservation.NameMaxLength} characters.", ErrorType.Validation);

    public static Error FullyRedeemed(int quantity)
        => new("Ticket.FullyRedeemed", $"All {quantity} are already in.", ErrorType.Conflict);

    public static Error Conflict()
        => new("Event.Conflict", "Those tickets just went. Check how many are left and try again.", ErrorType.Conflict);
}
