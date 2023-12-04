namespace system_reporter_v3;

using hardware_connetion_monitor;
using OneOf;

internal static class BusinessLogic
{
    // Pure function, does not edit parameters, uses pattern matching, returns discriminated union
    internal static OneOf<AddNew, UpdateLastEndTime, UpdateLastEndTimeAndAddNew> DetermineLogChanges(
        in HardwareConnectionStateChangedEvent e,
        in Disconnection? last) =>
        (last, e) switch
        {
            { last: null, e: _ } or { last.EndTime: not null, e: _ } => new AddNew(new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt)),
            { last: _, e.State: HardwareConnectionState.CONNECTED } => new UpdateLastEndTime(last, e.OccurredAt),
            _ => new UpdateLastEndTimeAndAddNew(last, e.OccurredAt, new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt))
        };

    internal sealed class AddNew(Disconnection newDisconnection)
    {
        public Disconnection Instance => newDisconnection;
    }

    internal sealed class UpdateLastEndTime(Disconnection last, DateTime endTime)
    {
        public Disconnection Last => last;
        public DateTime EndTime => endTime;
    }

    internal sealed class UpdateLastEndTimeAndAddNew(Disconnection last, DateTime endTime, Disconnection newDisconnection)
    {
        public Disconnection Last => last;
        public DateTime EndTime => endTime;
        public Disconnection NewInstance => newDisconnection;
    }
}

