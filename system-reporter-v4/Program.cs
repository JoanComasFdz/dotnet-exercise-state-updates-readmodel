using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using hardware_connetion_monitor;
using system_reporter_v4;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<HardwareConnectionState>()))
    .AddDbContext<DisconnectionsDBContext>(options => options.UseInMemoryDatabase("disconnections"));

var app = builder.Build();

app.MapGet("/", (DisconnectionsDBContext db) => Results.Ok(db.Disconnections.ToArray()));

app.MapPost("/", (HardwareConnectionStateChangedEvent e, DisconnectionsDBContext db) => 
{
    BusinessLogic.LogChanges(
        e,
        hardwareUnitId => db.Disconnections.LastOrDefault(d => d.HardwareUnitId == hardwareUnitId),
        newDisconnection => db.Disconnections.Add(newDisconnection),
        (last, newEndTime) => last.EndTime = newEndTime,
        (last, newEndTime, newDisconnection) =>
        {
            last.EndTime = newEndTime;
            db.Disconnections.Add(newDisconnection);
        }
        );
    db.SaveChanges();
});

app.Run();