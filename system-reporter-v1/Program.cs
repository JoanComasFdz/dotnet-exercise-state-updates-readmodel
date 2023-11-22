using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using hardware_connetion_monitor;
using system_reporter_v1;

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
            update.Last.EndTime = update.EndTime;
            break;
        case BusinessLogic.UpdateLastEndTimeAndAddNew updateAndAdd:
            updateAndAdd.Last.EndTime = updateAndAdd.EndTime;
            db.Disconnections.Add(updateAndAdd.NewInstance);
            break;
        default:
            throw new NotImplementedException($"No handling found for log change {logChange}.");
    };
    db.SaveChanges();
});

app.Run();