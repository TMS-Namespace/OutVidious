using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Apps.FrontTube.Backend.Repository.DataBase;
using TMS.Apps.FrontTube.Backend.Repository.DataBase.Entities;

namespace TMS.Apps.FrontTube.Backend.Repository.CacheManager;

/// <summary>
/// Seeds development data when IsDevMode is enabled.
/// </summary>
public sealed class DevModeSeeder
{
    /// <summary>
    /// The ID of the development user.
    /// </summary>
    public const int DevUserId = 1;

    /// <summary>
    /// The name of the development user.
    /// </summary>
    public const string DevUserName = "DevUser";

    private readonly ILogger<DevModeSeeder> _logger;

    public DevModeSeeder(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<DevModeSeeder>();
    }

    /// <summary>
    /// Seeds the development user if it doesn't exist.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The development user entity.</returns>
    public async Task<UserEntity> SeedDevUserAsync(DataBaseContext dbContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == DevUserId, cancellationToken);

        if (existingUser is not null)
        {
            _logger.LogDebug("Development user already exists: {UserId} - {UserName}", existingUser.Id, existingUser.Name);
            return existingUser;
        }

        var devUser = new UserEntity
        {
            Name = DevUserName,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(devUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Development user seeded: {UserId} - {UserName}", devUser.Id, devUser.Name);

        return devUser;
    }

    /// <summary>
    /// Gets the development user, creating it if necessary.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The development user entity, or null if not in dev mode context.</returns>
    public async Task<UserEntity?> GetOrCreateDevUserAsync(DataBaseContext dbContext, CancellationToken cancellationToken)
    {
        return await SeedDevUserAsync(dbContext, cancellationToken);
    }
}
