using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using hardware_connetion_monitor;
using system_reporter_v4;
using static system_reporter_v4.BusinessLogic;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<HardwareConnectionState>()))
    .AddDbContext<DisconnectionsDBContext>(options => options.UseInMemoryDatabase("disconnections"));

var app = builder.Build();

app.MapGet("/", (DisconnectionsDBContext db) => Results.Ok(db.Disconnections.ToArray()));

app.MapPost("/", (HardwareConnectionStateChangedEvent e, DisconnectionsDBContext db) =>
{
    var disconnectionOrDefault = db.Disconnections.LastOrDefault(d => d.HardwareUnitId == e.HardwareUnitId);

    var logChange = DetermineLogChanges(e, disconnectionOrDefault);
    logChange.Switch(
        newDisconnection => db.Disconnections.Add(newDisconnection),
        update => update.Last.EndTime = update.UpdatedEndTime,
        updateAndAdd =>
        {
            updateAndAdd.Last.EndTime = updateAndAdd.UpdatedEndTime;
            db.Disconnections.Add(updateAndAdd.NewDisconnection);
        });
    db.SaveChanges();
});

app.Run();