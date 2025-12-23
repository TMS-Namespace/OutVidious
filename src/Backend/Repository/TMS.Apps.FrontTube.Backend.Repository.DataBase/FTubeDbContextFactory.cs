using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TMS.Apps.FTube.Backend.DataBase;

/// <summary>
/// Design-time factory for creating FTubeDbContext for migrations.
/// </summary>
public class FTubeDbContextFactory : IDesignTimeDbContextFactory<FTubeDbContext>
{
    public FTubeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FTubeDbContext>();
        
        // Default connection string for development/migrations
        // In production, this should come from configuration
        var connectionString = "Host=localhost;Port=5656;Database=ftube;Username=root;Password=password";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new FTubeDbContext(optionsBuilder.Options);
    }
}
