using hardware_connetion_monitor;
using OneOf;

namespace system_reporter_v2;

internal static class BusinessLogic
{
    // Pure function, does not edit parameters, returns discriminated union
    internal static OneOf<AddNew, UpdateLastEndTime, UpdateLastEndTimeAndAddNew> DetermineLogChanges(
        in HardwareConnectionStateChangedEvent e,
        in Disconnection? last)
    {
        if (last is null || last.EndTime is not null)
        {
            var becauseNoneExists = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
            return new AddNew(becauseNoneExists);
        }

        if (e.State == HardwareConnectionState.CONNECTED)
        {
            return new UpdateLastEndTime(last, e.OccurredAt);
        }

        // Last exists and endtime is null and state is not CONNECTED
        var newDisconnection = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
        return new UpdateLastEndTimeAndAddNew(last, e.OccurredAt, newDisconnection);
    }

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

