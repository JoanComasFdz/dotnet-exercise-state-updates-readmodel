namespace system_reporter_oop;

public interface IDisconnectionRespository
{
    public Disconnection? GetLastByHardwareUnitId(string  hardwareUnitId);

    public void Add(Disconnection connection);

    public void Update(Disconnection connection);

}

internal class DisconnectionRepository : IDisconnectionRespository
{
    private readonly DisconnectionsDBContext db;

    public DisconnectionRepository(DisconnectionsDBContext db)
    {
        this.db = db;
    }

    public Disconnection? GetLastByHardwareUnitId(string hardwareUnitId)
    {
        return db.Disconnections.LastOrDefault(d => d.HardwareUnitId == hardwareUnitId);
    }

    public void Add(Disconnection disconnection)
    {
        db.Disconnections.Add(disconnection);
        db.SaveChanges();
    }

    public void Update(Disconnection disconnection)
    {
        db.Disconnections.Update(disconnection);
        db.SaveChanges();
    }

    // Potential optimization. Is the business logic leaking?
    public void UpdateAndAdd(Disconnection toUpdate, Disconnection toAdd)
    {
        db.Disconnections.Update(toUpdate);
        db.Disconnections.Add(toAdd);
        db.SaveChanges();
    }
}