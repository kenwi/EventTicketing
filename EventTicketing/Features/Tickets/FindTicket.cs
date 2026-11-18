using EventTicketing.Data;
using EventTicketing.Domain;
using EventTicketing.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Features.Tickets;

public static class FindTicket
{
    public sealed record Response(
        string Code,
        string EventName,
        EventKind Kind,
        DateTimeOffset StartsAt,
        string Venue,
        int Quantity,
        int TotalKr);

    public sealed class Handler(TicketingDbContext dbContext)
    {
        public async Task<Result<Response>> HandleAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            var normalized = code.Trim().ToUpperInvariant();
            var ticket = await (
                from reservation in dbContext.Reservations.AsNoTracking()
                join ev in dbContext.Events.AsNoTracking() on reservation.EventId equals ev.Id
                where reservation.Code == normalized
                select new Response(
                    reservation.Code,
                    ev.Name,
                    ev.Kind,
                    ev.StartsAt,
                    ev.Venue,
                    reservation.Quantity,
                    reservation.TotalKr))
                .FirstOrDefaultAsync(cancellationToken);

            if (ticket is null)
            {
                return EventErrors.TicketNotFound(normalized);
            }

            return ticket;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/{code}", async (
            string code,
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(code, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        });

    public static void MapPage(IEndpointRouteBuilder app)
    {
        app.MapGet("/tickets", (string? code) =>
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.Redirect("/");
            }

            return Results.Redirect($"/tickets/{code.Trim().ToUpperInvariant()}");
        });

        app.MapGet("/tickets/{code}", async (
            string code,
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(code, cancellationToken);
            if (!result.IsSuccess)
            {
                return (IResult)new RazorComponentResult<FindTicketPage>(new
                {
                    Message = result.Error!.Description
                });
            }

            return (IResult)new RazorComponentResult<FindTicketPage>(new
            {
                Ticket = result.Value
            });
        });
    }
}
