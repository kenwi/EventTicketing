namespace EventTicketing.Domain;

public static class EventErrors
{
    public static Error NotFound(int eventId) 
        => new("Event.NotFounSd", $"Event with ID {eventId} was not found.", ErrorType.NotFound);
    public static Error SoldOut(int ticketsLeft, int requested) 
        => new("Event.SoldOut", $"There are only {ticketsLeft} tickets left. You requested {requested}.", ErrorType.Conflict);
}