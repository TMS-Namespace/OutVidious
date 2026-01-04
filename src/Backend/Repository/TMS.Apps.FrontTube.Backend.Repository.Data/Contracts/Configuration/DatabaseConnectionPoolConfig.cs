namespace TMS.Apps.FrontTube.Backend.Repository.Data.Contracts.Configuration;

public sealed record DatabaseConnectionPoolConfig
{
    public bool Enabled { get; init; } = true;

    public int? MinPoolSize { get; init; } = 1;

    public int? MaxPoolSize { get; init; } = 20;

    public int? TimeoutSeconds { get; init; } = 15;

    public int? ConnectionIdleLifetimeSeconds { get; init; } = 300;

    public int? ConnectionPruningIntervalSeconds { get; init; } = 60;
}
