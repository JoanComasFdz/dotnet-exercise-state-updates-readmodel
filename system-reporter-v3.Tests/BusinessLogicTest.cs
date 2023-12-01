namespace system_reporter_v3.Tests;

using hardware_connetion_monitor;
using system_reporter_v3;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        Assert.Equal(typeof(BusinessLogic.AddNew), result.Value.GetType());
        var addNew = result.AsT0;
        Assert.Equal(expectedHardwarUnitId, addNew.Instance.HardwareUnitId);
        Assert.Equal(expectedState, addNew.Instance.State);
        Assert.Equal(expectedStartTime, addNew.Instance.StartTime);
        Assert.Null(addNew.Instance.EndTime);
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
        result.Switch(
            addNew =>
            {
                Assert.Equal(expectedHardwarUnitId, addNew.Instance.HardwareUnitId);
                Assert.Equal(expectedState, addNew.Instance.State);
                Assert.Equal(expectedStartTime, addNew.Instance.StartTime);
                Assert.Null(addNew.Instance.EndTime);
            },
            x => Assert.Fail($"Returned {x} instead of {nameof(BusinessLogic.AddNew)}"),
            x => Assert.Fail($"Returned {x} instead of {nameof(BusinessLogic.AddNew)}")
            );
    }
}