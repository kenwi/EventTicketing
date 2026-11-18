using EventTicketing.Data;
using EventTicketing.Domain;
using EventTicketing.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Features.Events;

public static class GetEvent
{
    public sealed record Response(
        int Id,
        string Name,
        EventKind Kind,
        DateTimeOffset StartsAt,
        string Venue,
        int PriceKr,
        string Hook,
        string Description,
        int Capacity,
        int TicketsLeft);

    public sealed class Handler(TicketingDbContext dbContext)
    {
        public async Task<Result<Response>> HandleAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            var ev = await dbContext.Events
                .AsNoTracking()
                .Where(e => e.Id == eventId)
                .Select(e => new Response(
                    e.Id,
                    e.Name,
                    e.Kind,
                    e.StartsAt,
                    e.Venue,
                    e.PriceKr,
                    e.Hook,
                    e.Description,
                    e.Capacity,
                    e.TicketsLeft))
                .FirstOrDefaultAsync(cancellationToken);

            if (ev is null)
            {
                return EventErrors.NotFound(eventId);
            }

            return ev;
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("/{eventId:int}", async (
            int eventId,
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(eventId, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        });

    public static void MapPage(IEndpointRouteBuilder app) =>
        app.MapGet("/events/{eventId:int}", async (
            int eventId,
            Handler handler,
            IAntiforgery antiforgery,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(eventId, cancellationToken);
            if (!result.IsSuccess)
            {
                return (IResult)new RazorComponentResult<MissingPage>(new
                {
                    Message = result.Error!.Description
                });
            }

            var tokens = antiforgery.GetAndStoreTokens(http);
            return (IResult)new RazorComponentResult<GetEventPage>(new
            {
                Model = result.Value,
                FormFieldName = tokens.FormFieldName,
                RequestToken = tokens.RequestToken
            });
        });
}
