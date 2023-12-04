namespace system_reporter_v3.Tests;

using hardware_connetion_monitor;
using system_reporter_v4;

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

        Assert.True(result.IsT0);
        Assert.Equal(expectedHardwarUnitId, result.AsT0.HardwareUnitId);
        Assert.Equal(expectedState, result.AsT0.State);
        Assert.Equal(expectedStartTime, result.AsT0.StartTime);
        Assert.Null(result.AsT0.EndTime);
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

        Assert.True(result.IsT0);
        Assert.Equal(expectedHardwarUnitId, result.AsT0.HardwareUnitId);
        Assert.Equal(expectedState, result.AsT0.State);
        Assert.Equal(expectedStartTime, result.AsT0.StartTime);
        Assert.Null(result.AsT0.EndTime);
    }
}