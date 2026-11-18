using System.Globalization;
using EventTicketing.Domain;

namespace EventTicketing.Web;

public static class Format
{
    private static readonly TimeSpan Oslo = TimeSpan.FromHours(1);

    public static string Kind(EventKind kind) => kind switch
    {
        EventKind.Lan => "LAN",
        _ => kind.ToString()
    };

    public static string When(DateTimeOffset startsAt)
    {
        var local = startsAt.ToOffset(Oslo);
        return local.ToString("d MMM · HH:mm", CultureInfo.InvariantCulture);
    }

    public static string Month(DateTimeOffset startsAt)
    {
        var local = startsAt.ToOffset(Oslo);
        return local.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
    }

    public static string Money(int kroner)
        => kroner == 0
            ? "Free"
            : string.Create(CultureInfo.GetCultureInfo("nb-NO"), $"{kroner:N0}") + " kr";

    public static string Left(int ticketsLeft)
        => ticketsLeft == 0 ? "Sold out" : $"{ticketsLeft} left";
}
