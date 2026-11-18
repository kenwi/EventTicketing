using EventTicketing.Data;
using EventTicketing.Domain;
using EventTicketing.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Features.Events;

public static class ListEvents
{
    public sealed record Response(
        int Id,
        string Name,
        EventKind Kind,
        DateTimeOffset StartsAt,
        string Venue,
        int PriceKr,
        string Hook,
        int TicketsLeft);

    public sealed class Handler(TicketingDbContext dbContext)
    {
        public async Task<IReadOnlyList<Response>> HandleAsync(
            EventKind? kind,
            CancellationToken cancellationToken = default)
        {
            var query = dbContext.Events.AsNoTracking();
            if (kind is not null)
            {
                query = query.Where(e => e.Kind == kind);
            }

            var events = await query
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.Kind,
                    e.StartsAt,
                    e.Venue,
                    e.PriceKr,
                    e.Hook,
                    e.TicketsLeft))
                .ToListAsync(cancellationToken);

            return events.OrderBy(e => e.StartsAt).ToList();
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/", async (EventKind? kind, Handler handler, CancellationToken cancellationToken) =>
            Results.Ok(await handler.HandleAsync(kind, cancellationToken)));

    public static void MapPage(IEndpointRouteBuilder app) =>
        app.MapGet("/", async (string? kind, Handler handler, CancellationToken cancellationToken) =>
        {
            EventKind? parsed = Enum.TryParse<EventKind>(kind, ignoreCase: true, out var value)
                ? value
                : null;

            var events = await handler.HandleAsync(parsed, cancellationToken);
            return new RazorComponentResult<ListEventsPage>(new
            {
                Events = events,
                ActiveKind = parsed
            });
        });
}
