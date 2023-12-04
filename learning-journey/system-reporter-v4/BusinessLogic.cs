namespace system_reporter_v4;

using hardware_connetion_monitor;
using OneOf;

internal static class BusinessLogic
{
    // Pure function, does not edit parameters, uses pattern matching, returns discriminated union, no custom types just tuples
    /// <summary>
    /// Analyzes the event and the last disconnections and returns the change that should be applied to the log:
    /// <list type="bullet">
    /// <item>Add a new disconnection</item>
    /// <item>Update end time on disconnection</item>
    /// <item>Update end time on disconnection and add a new disconnection</item>
    /// </list>
    /// <para>
    /// Example:
    /// <code>
    ///     var logChange = DetermineLogChanges(e, disconnectionOrDefault);
    ///     logChange.Switch(
    ///         newDisconnection => //Add to the DB,
    ///         update => update.Last.EndTime = update.UpdatedEndTime // Update in the DB,
    ///         updateAndAdd =>
    ///     {
    ///         updateAndAdd.Last.EndTime = updateAndAdd.UpdatedEndTime; // Update in the DB
    ///         //Add to the DB
    ///     });
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="e">The new event to be used to determine</param>
    /// <param name="last"></param>
    /// <returns>
    /// Just one out of the 3 possibilities
    /// <list type="bullet">
    /// <item>Just a <see cref="Disconnection"/> if it has to be added.</item>
    /// <item>The <see cref="Disconnection"/> passed in and a <see cref="DateTime"/> if it has to be updated.</item>
    /// <item>The <see cref="Disconnection"/> passed in, a <see cref="DateTime"/> and a new <see cref="Disconnection"/> if the passt in has to be
    /// updated and a new one created</item>
    /// </list>
    ///
    /// </returns>
    internal static OneOf<
        Disconnection,
        (Disconnection Last, DateTime UpdatedEndTime),
        (Disconnection Last, DateTime UpdatedEndTime, Disconnection NewDisconnection)>
        DetermineLogChanges(in HardwareConnectionStateChangedEvent e, in Disconnection? last) =>
        (last, e) switch
        {
            { last: null, e: _ } or { last.EndTime: not null, e: _ } => new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt),
            { last: _, e.State: HardwareConnectionState.CONNECTED } => (last, e.OccurredAt),
            _ => (last, e.OccurredAt, new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt))
        };
}

