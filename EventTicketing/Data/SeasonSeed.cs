using EventTicketing.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventTicketing.Data;

public static class SeasonSeed
{
    private static readonly TimeSpan Oslo = TimeSpan.FromHours(1);

    public static void Apply(EntityTypeBuilder<Event> builder)
    {
        builder.HasData(
            Row(1, "All-night Oslo", EventKind.Lan, At(2026, 11, 21, 12, 0), "Chateau Neuf", 249, 120,
                "Bring a rig. Leave when the sun does.",
                "Twelve hours of open play at Chateau Neuf. Bring your own machine, take a seat, and rotate through the brackets on the floor. Power and ethernet are on us. The doors stay open until morning."),
            Row(2, "Frontend Friday", EventKind.Meetup, At(2026, 11, 27, 17, 0), "Mesh", 0, 80,
                "One talk, then the room takes over.",
                "A free Friday at Mesh for people who ship interfaces. One short talk, then open demos until the building kicks us out. No price. A code still holds your spot."),
            Row(3, "How we shipped it", EventKind.Talk, At(2026, 12, 3, 18, 0), "Deichman Bjørvika", 149, 150,
                "The launch story, without the slide costume.",
                "Three Oslo teams tell the unpolished version of a ship: what broke, what they cut, and what they would skip next time. At Deichman Bjørvika. Questions are the point."),
            Row(4, "Game Dev Conference", EventKind.Conference, At(2026, 12, 4, 9, 0), "Rebel", 890, 100,
                "A full day on making games in this city.",
                "Talks and floor time for people building games in Norway. Design, engineering, and production, one day at Rebel. One hundred seats, and it will feel like it."),
            Row(5, "Demo night", EventKind.Show, At(2026, 12, 11, 20, 0), "Blå", 220, 280,
                "Realtime visuals, too loud for a meetup.",
                "Live demos at Blå: graphics, sound, and whatever else fits on a stage. Makers get a slot. Everyone else gets a dark room and a loud system. A show, not a standup meetup."),
            Row(6, "Rust på kvelden", EventKind.Meetup, At(2027, 1, 8, 18, 0), "Bitraf", 0, 40,
                "Borrow a chair. Bring a compile error.",
                "Evening Rust at Bitraf. Free, small, and practical. Beginners and people who already argue about lifetimes share one room. Bring a project, or sit in and steal one."),
            Row(7, "AI & Machine Learning Summit", EventKind.Conference, At(2027, 1, 14, 9, 0), "Rebel", 1290, 200,
                "Models, products, and the mess between them.",
                "A day at Rebel for people putting machine learning into real products. Research in the morning, builder stories after lunch. Two hundred seats, then the doors close."),
            Row(8, "GPUs for the rest of us", EventKind.Talk, At(2027, 1, 22, 18, 0), "Sentralen", 199, 180,
                "If the cluster is not yours, start here.",
                "One evening on using GPUs without a research budget. Renting, sizing, and the mistakes that burn a weekend. Sentralen, one track, questions in the room."),
            Row(9, "Winter spawn", EventKind.Lan, At(2027, 2, 6, 12, 0), "Chateau Neuf", 349, 160,
                "Same hall. Colder weekend. Faster lobby.",
                "The winter LAN at Chateau Neuf. Bring a rig, join the brackets, and stay through the late session. One hundred and sixty seats on the floor."),
            Row(10, "Nordic Platform Day", EventKind.Conference, At(2027, 2, 19, 9, 0), "Rebel", 1490, 350,
                "For the people who keep the platform up.",
                "A day at Rebel for platform, infra, and developer-experience teams from across the Nordics. Three hundred and fifty seats. Talks from the people who run the systems, not a product keynote."),
            Row(11, "Synth and silicon", EventKind.Show, At(2027, 2, 27, 20, 0), "Rockefeller", 390, 450,
                "Machines on stage. Volume up.",
                "An evening at Rockefeller where hardware and music share the bill. Live sets on synths, code, and whatever else the acts plug in. A show first, a tech event second."),
            Row(12, "Design systems drop-in", EventKind.Meetup, At(2027, 3, 5, 17, 0), "Vippa", 0, 60,
                "Bring a component. Steal two.",
                "A free drop-in at Vippa for people who maintain a design system. Show a component, trade a token decision, leave with someone else's fix. Sixty spots, then the list closes.")
        );
    }

    private static DateTimeOffset At(int year, int month, int day, int hour, int minute)
        => new(year, month, day, hour, minute, 0, Oslo);

    private static object Row(
        int id,
        string name,
        EventKind kind,
        DateTimeOffset startsAt,
        string venue,
        int priceKr,
        int capacity,
        string hook,
        string description)
        => new
        {
            Id = id,
            Name = name,
            Kind = kind,
            StartsAt = startsAt,
            Venue = venue,
            PriceKr = priceKr,
            Hook = hook,
            Description = description,
            Capacity = capacity,
            TicketsSold = 0,
            Version = 0
        };
}
