using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Auth;

/// <summary>
/// Allowlist of active refresh token jti's. Redis key is "refresh:{jti}" -> owning user id,
/// with a TTL matching the refresh token lifetime. Deleting the key revokes it immediately
/// (used for logout and rotation) even though the JWT itself would still validate.
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

    public Task RegisterAsync(string jti, Guid userId, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        return db.StringSetAsync(KeyPrefix + jti, userId.ToString(), TimeSpan.FromDays(_settings.ExpiryDays));
    }

    public async Task<bool> IsActiveAsync(string jti, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(KeyPrefix + jti);
    }

    public Task RevokeAsync(string jti, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        return db.KeyDeleteAsync(KeyPrefix + jti);
    }
}
