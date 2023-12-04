namespace system_reporter_v6.Tests;

using hardware_connetion_monitor;
using system_reporter_functional;

public class BusinessLogicTest
{
    [Fact]
    public void DetermineLogChanges_LastDoesntExist_CreatesNew()
    {
        var expectedHardwarUnitId = "u1";
        var expectedState = HardwareConnectionState.DISCONNECTED;
        var expectedStartTime = DateTime.Now;
        var e = new HardwareConnectionStateChangedEvent(expectedHardwarUnitId, expectedState, expectedStartTime);

        var result = BusinessLogic.DetermineLogChanges(e, null);

        result.MatchAddNew(
            addNew =>
            {
                Assert.Equal(expectedHardwarUnitId, addNew.NewDisconnection.HardwareUnitId);
                Assert.Equal(expectedState, addNew.NewDisconnection.State);
                Assert.Equal(expectedStartTime, addNew.NewDisconnection.StartTime);
                Assert.Null(addNew.NewDisconnection.EndTime);
            },
            () => Assert.Fail($"Expected 'AddNew' but result is '{result.GetType().Name}")
            );

        // Alternative, do not count as lines
        var expectedDisconnection = new Disconnection(expectedHardwarUnitId, expectedState, expectedStartTime);
        result.MatchAddNew(
            addNew => Assert.Equal(expectedDisconnection, addNew.NewDisconnection),
            () => Assert.Fail($"Expected 'AddNew' but result is '{result.GetType().Name}")
            );
    }

    [Fact]
    public void DetermineLogChanges_EventIsDisconnectedAndLastHasEndTime_CreatesNew()
    {
        var expectedHardwarUnitId = "u1";
        var expectedState = HardwareConnectionState.DISCONNECTED;
        var expectedStartTime = DateTime.Now;
        var e = new HardwareConnectionStateChangedEvent(expectedHardwarUnitId, expectedState, expectedStartTime);

        var yesterday = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        var now = DateTime.Now;
        var last = new Disconnection(expectedHardwarUnitId, HardwareConnectionState.WAITING, yesterday)
        {
            EndTime = now,
        };

        var result = BusinessLogic.DetermineLogChanges(e, last);

        result.MatchAddNew(
            addNew =>
            {
                Assert.Equal(expectedHardwarUnitId, addNew.NewDisconnection.HardwareUnitId);
                Assert.Equal(expectedState, addNew.NewDisconnection.State);
                Assert.Equal(expectedStartTime, addNew.NewDisconnection.StartTime);
                Assert.Null(addNew.NewDisconnection.EndTime);
            },
            () => Assert.Fail($"Expected 'AddNew' but result is '{result.GetType().Name}")
            );

        // Alternative, do not count as lines
        var expectedDisconnection = new Disconnection(expectedHardwarUnitId, expectedState, expectedStartTime);
        Assert.Equal(expectedDisconnection, result.UnwrapAddNew().NewDisconnection);

        // Alternative, do not count as lines
        var addNew = result as LogChange.AddNew;
        Assert.IsType<LogChange.AddNew>(addNew);
        Assert.Equal(expectedDisconnection, addNew.NewDisconnection);

    }
}