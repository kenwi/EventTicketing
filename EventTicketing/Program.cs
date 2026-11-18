using EventTicketing.Data;
using EventTicketing.Features.Events;
using EventTicketing.Features.Tickets;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TicketingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Ticketing")));

builder.Services.AddScoped<ListEvents.Handler>();
builder.Services.AddScoped<GetEvent.Handler>();
builder.Services.AddScoped<BuyTickets.Handler>();
builder.Services.AddScoped<FindTicket.Handler>();

builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddAntiforgery();
builder.Services.AddRazorComponents();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
    dbContext.Database.EnsureCreated();
}

var events = app.MapGroup("/api/events");
ListEvents.MapEndpoint(events);
GetEvent.MapEndpoint(events);
BuyTickets.MapEndpoint(events);

FindTicket.MapEndpoint(app.MapGroup("/api/tickets"));

ListEvents.MapPage(app);
GetEvent.MapPage(app);
BuyTickets.MapPage(app);
FindTicket.MapPage(app);

app.Run();
