using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using hardware_connetion_monitor;
using system_reporter;

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

    switch (logChange)
    {
        case BusinessLogic.AddNew addNew:
            db.Disconnections.Add(addNew.Instance);
            break;
        case BusinessLogic.UpdateLastEndTime update:
            SetLastDisconnectionEndTime(db, update.HardwareUnitId, update.EndTime);
            break;
        case BusinessLogic.UpdateLastEndTimeAndAddNew updateAndAdd:
            SetLastDisconnectionEndTime(db, updateAndAdd.HardwareUnitId, updateAndAdd.EndTime);
            db.Disconnections.Add(updateAndAdd.NewInstance);
            break;
    };
    db.SaveChanges();

    void SetLastDisconnectionEndTime(DisconnectionsDBContext db, string hardwareUnitId, DateTime endTime)
    {
        var last = db.Disconnections.Last(d => d.HardwareUnitId == hardwareUnitId);
        last.EndTime = endTime;
    }
});

app.Run();