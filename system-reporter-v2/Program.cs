using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using hardware_connetion_monitor;
using system_reporter_v2;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<HardwareConnectionState>()))
    .AddDbContext<DisconnectionsDBContext>(options => options.UseInMemoryDatabase("disconnections"));

var app = builder.Build();

app.MapGet("/", (DisconnectionsDBContext db) => Results.Ok(db.Disconnections.ToArray()));

app.MapPost("/", (HardwareConnectionStateChangedEvent e, DisconnectionsDBContext db) => 
{
    var disconnectionOrDefault = db.Disconnections.LastOrDefault(d => d.HardwareUnitId == e.HardwareUnitId);

    var logChange = BusinessLogic.DetermineLogChanges(e, disconnectionOrDefault);
    logChange.Switch(
        addNew => db.Disconnections.Add(addNew.Instance),
        update => update.last.EndTime = update.EndTime,
        updateAndAdd =>
        {
            updateAndAdd.last.EndTime = updateAndAdd.EndTime;
            db.Disconnections.Add(updateAndAdd.NewInstance);
        }
        );
    db.SaveChanges();
});

app.Run();