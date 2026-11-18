using System.ComponentModel.DataAnnotations;
using EventTicketing.Data;
using EventTicketing.Domain;
using EventTicketing.Features.Events;
using EventTicketing.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Features.Tickets;

public static class BuyTickets
{
    public sealed record Request([property: Range(1, 20)] int Quantity);

    public sealed record Response(
        string Code,
        int EventId,
        string EventName,
        int Quantity,
        int TotalKr,
        int TicketsLeft);

    public sealed class Handler(TicketingDbContext dbContext)
    {
        public async Task<Result<Response>> HandleAsync(
            int eventId,
            int quantity,
            CancellationToken cancellationToken = default)
        {
            var ev = await dbContext.Events
                .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

            if (ev is null)
            {
                return EventErrors.NotFound(eventId);
            }

            if (quantity is < 1 or > 20)
            {
                return EventErrors.QuantityOutOfRange();
            }

            var payment = TakePayment.Charge(ev.PriceKr * quantity);
            if (!payment.IsSuccess)
            {
                return payment.Error!;
            }

            var code = await NextCodeAsync(cancellationToken);
            var sold = ev.Sell(quantity, code, DateTimeOffset.UtcNow);
            if (!sold.IsSuccess)
            {
                return sold.Error!;
            }

            dbContext.Reservations.Add(sold.Value);
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

            return new Response(code, ev.Id, ev.Name, quantity, ev.PriceKr * quantity, ev.TicketsLeft);
        }

        private async Task<string> NextCodeAsync(CancellationToken cancellationToken)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var chars = new char[6];
                for (var i = 0; i < chars.Length; i++)
                {
                    chars[i] = alphabet[Random.Shared.Next(alphabet.Length)];
                }

                var code = $"HZ-{new string(chars)}";
                var taken = await dbContext.Reservations
                    .AnyAsync(r => r.Code == code, cancellationToken);
                if (!taken)
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Could not issue a ticket code.");
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("/{eventId:int}/tickets", async (
            int eventId,
            Request request,
            Handler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(eventId, request.Quantity, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
        });

    public static void MapPage(IEndpointRouteBuilder app) =>
        app.MapPost("/events/{eventId:int}/tickets", async (
            int eventId,
            [FromForm] int quantity,
            Handler handler,
            GetEvent.Handler events,
            IAntiforgery antiforgery,
            HttpContext http,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(eventId, quantity, cancellationToken);
            if (result.IsSuccess)
            {
                return Results.Redirect($"/tickets/{result.Value.Code}");
            }

            var page = await events.HandleAsync(eventId, cancellationToken);
            if (!page.IsSuccess)
            {
                return (IResult)new RazorComponentResult<MissingPage>(new
                {
                    Message = page.Error!.Description
                });
            }

            var tokens = antiforgery.GetAndStoreTokens(http);
            return (IResult)new RazorComponentResult<GetEventPage>(new
            {
                Model = page.Value,
                Error = result.Error!.Description,
                FormFieldName = tokens.FormFieldName,
                RequestToken = tokens.RequestToken
            });
        });

    public static class TakePayment
    {
        public static Result Charge(int totalKr)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(totalKr);
            return Result.Success();
        }
    }
}
