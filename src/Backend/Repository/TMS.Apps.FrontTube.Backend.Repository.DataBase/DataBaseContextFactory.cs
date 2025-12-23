using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TMS.Apps.FrontTube.Backend.Repository.DataBase;

/// <summary>
/// Design-time factory for creating DataBaseContext for migrations.
/// </summary>
public class DataBaseContextFactory : IDesignTimeDbContextFactory<DataBaseContext>
{
    public DataBaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataBaseContext>();
        
        // Default connection string for development/migrations
        // In production, this should come from configuration
        var connectionString = "Host=localhost;Port=5656;Database=ftube;Username=root;Password=password";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new DataBaseContext(optionsBuilder.Options);
    }
}
