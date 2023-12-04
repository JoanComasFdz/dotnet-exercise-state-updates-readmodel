using hardware_connetion_monitor;

namespace system_reporter_v5;

internal static class BusinessLogic
{
    // Not Pure function anymore because it cannot xontrol that the lambdas won't have side effects.
    // does not edit parameters
    internal static void LogChanges(
        in HardwareConnectionStateChangedEvent e,
        GetLastDisconnectionByHardwareUnitIdFunc getLastDisconnectionByHardwareUnitId,
        AddNewAction addNew,
        UpdateLastEndTimeAction updateLastEndTime,
        UpdateLastEndTimeAndAddNewAction updateLastEndTimeAndAddNew)
    {
        var last = getLastDisconnectionByHardwareUnitId(e.HardwareUnitId);
        if (last is null || last.EndTime is not null)
        {
            var becauseNoneExists = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
            addNew(becauseNoneExists);
            return;
        }

        if (e.State == HardwareConnectionState.CONNECTED)
        {
            updateLastEndTime(last, e.OccurredAt);
            return;
        }

        // Last exists and endtime is null and state is not CONNECTED
        var newDisconnection = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
        updateLastEndTimeAndAddNew(last, e.OccurredAt, newDisconnection);
    }


    /// <summary>
    /// Delegate for getting the last disconnection by hardware unit ID.
    /// </summary>
    /// <param name="hardwareUnitId">Hardware unit ID to search for.</param>
    public delegate Disconnection? GetLastDisconnectionByHardwareUnitIdFunc(string hardwareUnitId);

    /// <summary>
    /// Delegate for adding a new disconnection.
    /// </summary>
    /// <param name="newDisconnection">The new disconnection to add.</param>
    public delegate void AddNewAction(Disconnection newDisconnection);

    /// <summary>
    /// Delegate for updating the end time of the last disconnection.
    /// </summary>
    /// <param name="lastDisconnection">The last disconnection to update.</param>
    /// <param name="newEndTime">The new end time to set.</param>
    public delegate void UpdateLastEndTimeAction(Disconnection lastDisconnection, DateTime newEndTime);

    /// <summary>
    /// Delegate for updating the last end time and adding a new disconnection.
    /// </summary>
    /// <param name="lastDisconnection">The last disconnection to update.</param>
    /// <param name="newEndTime">The new end time to set.</param>
    /// <param name="newDisconnection">The new disconnection to add.</param>
    public delegate void UpdateLastEndTimeAndAddNewAction(Disconnection lastDisconnection, DateTime newEndTime, Disconnection newDisconnection);

}