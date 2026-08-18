using Application.Common.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Auth;

/// <summary>
/// Denylist of revoked access-token jti's. Redis key is "blacklist:{jti}", TTL set to the
/// token's remaining lifetime so entries self-expire once the token would have expired anyway.
/// </summary>
public class RedisTokenBlacklist : ITokenBlacklist
{
    private const string KeyPrefix = "blacklist:";

    private readonly IConnectionMultiplexer _redis;

    public RedisTokenBlacklist(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public Task BlacklistAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        var ttl = expiresAtUtc - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        var db = _redis.GetDatabase();
        return db.StringSetAsync(KeyPrefix + jti, "1", ttl);
    }

    public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(KeyPrefix + jti);
    }
}
