namespace system_reporter_v6;

using hardware_connetion_monitor;
using Dunet;

internal static class BusinessLogic
{
    // Pure function, does not edit parameters, uses pattern matching, returns discriminated union
    internal static LogChange DetermineLogChanges(
        in HardwareConnectionStateChangedEvent e,
        in Disconnection? last) =>
        (last, e) switch
        {
            { last: null, e: _ } or { last.EndTime: not null, e: _ } => new LogChange.AddNew(new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt)),
            { last: _, e.State: HardwareConnectionState.CONNECTED } => new LogChange.UpdateLastEndTime(last, e.OccurredAt),
            _ => new LogChange.UpdateLastEndTimeAndAddNew(last, e.OccurredAt, new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt))
        };
}

[Union]
internal partial record LogChange
{
    partial record AddNew(Disconnection NewDisconnection);
    partial record UpdateLastEndTime(Disconnection Last, DateTime EndTime);
    partial record UpdateLastEndTimeAndAddNew(Disconnection Last, DateTime EndTime, Disconnection NewDisconnection);
}

