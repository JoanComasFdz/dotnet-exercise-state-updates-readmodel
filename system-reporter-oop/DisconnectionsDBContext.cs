using Microsoft.EntityFrameworkCore;

namespace system_reporter_oop;

internal class DisconnectionsDBContext(DbContextOptions<DisconnectionsDBContext> options) : DbContext(options)
{
    public DbSet<Disconnection> Disconnections { get; set; }
}