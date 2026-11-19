using EventTicketing.Data;
using EventTicketing.Domain;
using EventTicketing.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Features.Tickets;

public static class RedeemTicket
{
    public sealed record Response(
        string Code,
        string HolderName,
        string EventName,
        int Quantity,
        int RedeemedCount);

    public sealed class Handler(TicketingDbContext dbContext)
    {
        public async Task<Result<Response>> HandleAsync(
            string code,
            CancellationToken cancellationToken = default)
        {
            var normalized = code.Trim().ToUpperInvariant();
            var reservation = await dbContext.Reservations
                .FirstOrDefaultAsync(r => r.Code == normalized, cancellationToken);

            if (reservation is null)
            {
                return EventErrors.TicketNotFound(normalized);
            }

            var redeemed = reservation.Redeem();
            if (!redeemed.IsSuccess)
            {
                return redeemed.Error!;
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync(cancellationToken);
                }

                return EventErrors.Conflict();
            }

            var eventName = await dbContext.Events
                .Where(e => e.Id == reservation.EventId)
                .Select(e => e.Name)
                .FirstAsync(cancellationToken);

            return new Response(
                reservation.Code,
                reservation.HolderName,
                eventName,
                reservation.Quantity,
                reservation.RedeemedCount);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/{code}/redeem", async (
            string code,
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(code, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        });

    public static void MapPage(IEndpointRouteBuilder app) =>
        app.MapPost("/tickets/{code}/redeem", async (
            string code,
            Handler handler,
            FindTicket.Handler tickets,
            IAntiforgery antiforgery,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(code, cancellationToken);
            if (result.IsSuccess)
            {
                return Results.Redirect($"/tickets/{result.Value.Code}");
            }

            var page = await tickets.HandleAsync(code, cancellationToken);
            if (!page.IsSuccess)
            {
                return (IResult)new RazorComponentResult<FindTicketPage>(new
                {
                    Message = page.Error!.Description
                });
            }

            var tokens = antiforgery.GetAndStoreTokens(http);
            return (IResult)new RazorComponentResult<FindTicketPage>(new
            {
                Ticket = page.Value,
                Error = result.Error!.Description,
                FormFieldName = tokens.FormFieldName,
                RequestToken = tokens.RequestToken
            });
        });
}