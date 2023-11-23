using hardware_connetion_monitor;

namespace system_reporter_v3.Tests
{
    public class BusinessLogicTest
    {
        [Fact]
        public void LogChanges_LastDoesntExists_AddsNew()
        {
            var expectedHardwarUnitId = "u1";
            var expectedState = HardwareConnectionState.DISCONNECTED;
            var expectedStartTime = DateTime.Now;
            var e = new HardwareConnectionStateChangedEvent(expectedHardwarUnitId, expectedState, expectedStartTime);

            bool getLastCalled = false;
            Disconnection? newDisconnection = null;
            bool updateLastCalled = false;
            bool updateAndAddCalled= false;

            BusinessLogic.LogChanges(e,
                hid => { getLastCalled = true; return null; },
                newD => newDisconnection = newD,
                (last, time) => updateLastCalled = true,
                (last, time, newD) => updateAndAddCalled = true
                );

            Assert.True(getLastCalled);
            Assert.NotNull(newDisconnection);
            Assert.Equal(expectedHardwarUnitId, newDisconnection.HardwareUnitId);
            Assert.Equal(expectedState, newDisconnection.State);
            Assert.Equal(expectedStartTime, newDisconnection.StartTime);
            Assert.Null(newDisconnection.EndTime);

            Assert.False(updateLastCalled);
            Assert.False(updateAndAddCalled); 
        }
    }
}