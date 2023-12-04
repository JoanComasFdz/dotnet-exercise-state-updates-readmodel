using Microsoft.EntityFrameworkCore;

namespace system_reporter_functional;

public class DisconnectionsDBContext(DbContextOptions<DisconnectionsDBContext> options) : DbContext(options)
{
    public DbSet<Disconnection> Disconnections { get; set; }
}