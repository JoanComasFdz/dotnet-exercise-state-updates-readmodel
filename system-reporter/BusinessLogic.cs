using hardware_connetion_monitor;

namespace system_reporter;

internal static class BusinessLogic
{
    // Pure function, does not edit parameters, returns discriminated union
    internal static LogChange DetermineLogChanges(HardwareConnectionStateChangedEvent e, Disconnection? last)
    {
        if (last is null)
        {
            var becauseNoneExists = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
            return new AddNew(becauseNoneExists);
        }

        if (last.EndTime is not null)
        {
            var becaseLastHasEndTime = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
            return new AddNew(becaseLastHasEndTime);
        }

        if (e.State == HardwareConnectionState.CONNECTED)
        {
            return new UpdateLastEndTime(last.HardwareUnitId, e.OccurredAt);
        }

        // Last exists and endtime is null and state is not CONNECTED
        var newDisconnection = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
        return new UpdateLastEndTimeAndAddNew(last.HardwareUnitId, e.OccurredAt, newDisconnection);
    }

    internal abstract record LogChange
    {
    }

    internal sealed record AddNew(Disconnection newDisconnection) : LogChange
    {
        public Disconnection Instance => newDisconnection;
    }

    internal sealed record UpdateLastEndTime(string hardwareUnitId, DateTime endTime) : LogChange
    {
        public string HardwareUnitId => hardwareUnitId;
        public DateTime EndTime => endTime;
    }

    internal sealed record UpdateLastEndTimeAndAddNew(string hardwareUnitId, DateTime endTime, Disconnection newDisconnection) : LogChange
    {
        public string HardwareUnitId => hardwareUnitId;
        public DateTime EndTime => endTime;
        public Disconnection NewInstance => newDisconnection;
    }
}

