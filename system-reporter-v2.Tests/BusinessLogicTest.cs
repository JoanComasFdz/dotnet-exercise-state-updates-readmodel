namespace system_reporter_v2.Tests;

using hardware_connetion_monitor;
using system_reporter_v2;

public class BusinessLogicTest
{
    [Fact]
    public void DetermineLogChanges_LastDoesntExist_CreatesNew()
    {
        var now = DateTime.Now;
        var e = new HardwareConnectionStateChangedEvent("u1", HardwareConnectionState.DISCONNECTED, now);
        
        var result = BusinessLogic.DetermineLogChanges(e, null);

        Assert.True(result.IsT0);
        Assert.Equal("u1", result.AsT0.Instance.HardwareUnitId);
        Assert.Equal(HardwareConnectionState.DISCONNECTED, result.AsT0.Instance.State);
        Assert.Equal(now, result.AsT0.Instance.StartTime);
        Assert.Null(result.AsT0.Instance.EndTime);
    }
}