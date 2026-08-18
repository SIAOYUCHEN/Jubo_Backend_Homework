using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Auth;

/// <summary>
/// Refresh tokens are opaque GUIDs. Redis key is "refresh:{jti}" -> value is the owning
/// user id, with a TTL matching the refresh token lifetime. Deleting the key revokes it
/// immediately (used for logout and rotation).
/// </summary>
public class RedisRefreshTokenStore : IRefreshTokenStore
{
    private const string KeyPrefix = "refresh:";

    private readonly IConnectionMultiplexer _redis;
    private readonly RefreshTokenSettings _settings;

    public RedisRefreshTokenStore(IConnectionMultiplexer redis, IOptions<RefreshTokenSettings> settings)
    {
        _redis = redis;
        _settings = settings.Value;
    }

    public async Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken)
    {
        var jti = Guid.NewGuid().ToString("N");
        var db = _redis.GetDatabase();
        await db.StringSetAsync(KeyPrefix + jti, userId.ToString(), TimeSpan.FromDays(_settings.ExpiryDays));
        return jti;
    }

    public async Task<Guid?> GetUserIdAsync(string token, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(KeyPrefix + token);
        return value.HasValue && Guid.TryParse(value.ToString(), out var userId) ? userId : null;
    }

    public Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        return db.KeyDeleteAsync(KeyPrefix + token);
    }
}
