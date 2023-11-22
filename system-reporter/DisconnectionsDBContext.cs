using Microsoft.EntityFrameworkCore;

namespace system_reporter;

public class DisconnectionsDBContext(DbContextOptions<DisconnectionsDBContext> options) : DbContext(options)
{
    public DbSet<Disconnection> Disconnections { get; set; }
}