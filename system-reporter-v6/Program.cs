using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using hardware_connetion_monitor;
using system_reporter_v6;
using static system_reporter_v6.BusinessLogic;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<HardwareConnectionState>()))
    .AddDbContext<DisconnectionsDBContext>(options => options.UseInMemoryDatabase("disconnections"));

var app = builder.Build();

app.MapGet("/", (DisconnectionsDBContext db) => Results.Ok(db.Disconnections.ToArray()));

app.MapPost("/", (HardwareConnectionStateChangedEvent e, DisconnectionsDBContext db) =>
{
    // Impure
    var disconnectionOrDefault = db.Disconnections.LastOrDefault(d => d.HardwareUnitId == e.HardwareUnitId);

    // Pure
    var logChange = DetermineLogChanges(e, disconnectionOrDefault);

    // Impure
    logChange.Match(
        addNew => db.Disconnections.Add(addNew.NewDisconnection),
        update => update.Last.EndTime = update.EndTime,
        updateAndAdd =>
        {
            updateAndAdd.Last.EndTime = updateAndAdd.EndTime;
            db.Disconnections.Add(updateAndAdd.NewDisconnection);
        });
    db.SaveChanges();
});

app.Run();