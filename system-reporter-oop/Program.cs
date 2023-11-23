using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using hardware_connetion_monitor;
using system_reporter_oop;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<HardwareConnectionState>()))
    .AddDbContext<DisconnectionsDBContext>(options => options.UseInMemoryDatabase("disconnections"));

builder.Services.AddScoped<IDisconnectionRespository, DisconnectionRepository>();
builder.Services.AddScoped<ILogService, LogService>();

var app = builder.Build();

app.MapGet("/", (DisconnectionsDBContext db) => Results.Ok(db.Disconnections.ToArray()));

// Alternative 1: 
app.MapPost("/1", (HardwareConnectionStateChangedEvent e, LogService logUpdater) => 
{
    logUpdater.UpdateLog(e);
});

// Alternative 2
app.MapPost("/2", (HardwareConnectionStateChangedEvent e, DisconnectionsDBContext db) =>
{
    var last = db.Disconnections.LastOrDefault(d => d.HardwareUnitId == e.HardwareUnitId);

    if (last is null)
    {
        var becauseNoneExists = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
        db.Disconnections.Add(becauseNoneExists);
    }
    else if (last.EndTime is not null)
    {
        var becaseLastHasEndTime = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
        db.Disconnections.Add(becaseLastHasEndTime);
    }
    else if (e.State == HardwareConnectionState.CONNECTED)
    {
        last.EndTime = e.OccurredAt;
    }
    else
    {
        // Last exists and endtime is null and state is not CONNECTED
        last.EndTime = e.OccurredAt;
        var newDisconnection = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
        db.Disconnections.Add(newDisconnection);
    }

    db.SaveChanges();
});

app.Run();