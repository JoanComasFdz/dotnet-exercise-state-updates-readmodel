using hardware_connetion_monitor;
using NSubstitute;

namespace system_reporter_oop.Tests
{
    public class LogServiceTest
    {
        [Fact]
        public void UpdateLog_DisconnectionDoesntExist_CreatesNew()
        {
            var expectedHardwarUnitId = "u1";
            var expectedState = HardwareConnectionState.DISCONNECTED;
            var expectedStartTime = DateTime.Now;
            var e = new HardwareConnectionStateChangedEvent(expectedHardwarUnitId, expectedState, expectedStartTime);

            var repo = Substitute.For<IDisconnectionRespository>();

            var sut = new LogService(repo);

            sut.UpdateLog(e);

            repo.Received().GetLastByHardwareUnitId(expectedHardwarUnitId);
            repo.Received().Add(Arg.Is<Disconnection>(d => 
                d.HardwareUnitId == expectedHardwarUnitId
                && d.StartTime == expectedStartTime
                && d.State == expectedState
                && d.EndTime == null));
        }

        [Fact]
        public void UpdateLog_EventIsDisconnectedAndLastHasEndTime_CreatesNew()
        {
            var expectedHardwarUnitId = "u1";
            var expectedState = HardwareConnectionState.DISCONNECTED;
            var expectedStartTime = DateTime.Now;
            var e = new HardwareConnectionStateChangedEvent(expectedHardwarUnitId, expectedState, expectedStartTime);

            var yesterday = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            var last = new Disconnection(expectedHardwarUnitId, HardwareConnectionState.WAITING, yesterday)
            {
                EndTime = null,
            };

            var repo = Substitute.For<IDisconnectionRespository>();
            repo.GetLastByHardwareUnitId(expectedHardwarUnitId)
                .Returns(last);

            var sut = new LogService(repo);
            sut.UpdateLog(e);

            repo.Received().GetLastByHardwareUnitId(expectedHardwarUnitId);
            repo.Received().Add(Arg.Is<Disconnection>(d =>
                  d.HardwareUnitId == expectedHardwarUnitId
                  && d.StartTime == expectedStartTime
                  && d.State == expectedState
                  && d.EndTime == null));
        }
    }
}