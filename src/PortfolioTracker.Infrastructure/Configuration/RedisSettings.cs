namespace PortfolioTracker.Infrastructure.Configuration;

/// <summary>
/// Redis connection configuration.
/// Populated from appsettings.json "Redis" section.
/// </summary>
public class RedisSettings
{
    /// <summary>
    /// Redis connection string.
    /// Format: "host:port" or "host:port,password=xxx" for authenticated Redis
    /// </summary>
    /// <example>
    /// Development: "localhost:6379"
    /// Production: "your-redis-host.com:6379,password=your_secure_password"
    /// </example>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Instance name prefix for all Redis keys.
    /// Allows multiple applications to share same Redis instance without key collisions.
    /// </summary>
    /// <example>
    /// InstanceName = "PortfolioTracker:" 
    /// -> Key "AAPL:quote" becomes "PortfolioTracker:AAPL:quote" in Redis
    /// </example>
    public string InstanceName { get; set; } = "PortfolioTracker:";
}