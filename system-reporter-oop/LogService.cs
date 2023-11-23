using hardware_connetion_monitor;

namespace system_reporter_oop;

internal interface ILogService
{
   void UpdateLog(HardwareConnectionStateChangedEvent e);
}

internal class LogService : ILogService
{
    private readonly IDisconnectionRespository repo;

    public LogService(IDisconnectionRespository repo)
    {
        this.repo = repo;
    }

    public void UpdateLog(HardwareConnectionStateChangedEvent e)
    {
        var last = repo.GetLastByHardwareUnitId(e.HardwareUnitId);

        if (last is null)
        {
            var becauseNoneExists = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
            repo.Add(becauseNoneExists);
        } else if (last.EndTime is not null)
        {
            var becaseLastHasEndTime = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
            repo.Add(becaseLastHasEndTime);
        }
        else if (e.State == HardwareConnectionState.CONNECTED)
        {
            last.EndTime = e.OccurredAt;
            repo.Update(last);
        } else
        {
            // Last exists and endtime is null and state is not CONNECTED
            last.EndTime = e.OccurredAt;
            repo.Update(last);

            var newDisconnection = new Disconnection(e.HardwareUnitId, e.State, e.OccurredAt);
            repo.Add(newDisconnection);
        }
    }
}

