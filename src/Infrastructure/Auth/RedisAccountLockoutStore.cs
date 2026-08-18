using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Auth;

/// <summary>
/// Redis key "login-fail:{username}" holds the failed-attempt count. TTL is set to the
/// lockout window on the first failure and left alone after that, so it doubles as both
/// the attempt counter and the "time until unlock" — reaching the threshold before the
/// window expires is what makes the account locked.
/// </summary>
public class RedisAccountLockoutStore : IAccountLockoutStore
{
    private const string KeyPrefix = "login-fail:";

    private readonly IConnectionMultiplexer _redis;
    private readonly AccountLockoutSettings _settings;

    public RedisAccountLockoutStore(IConnectionMultiplexer redis, IOptions<AccountLockoutSettings> settings)
    {
        _redis = redis;
        _settings = settings.Value;
    }

    public async Task RegisterFailedAttemptAsync(string username, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var key = Key(username);

        var count = await db.StringIncrementAsync(key);
        if (count == 1)
        {
            await db.KeyExpireAsync(key, TimeSpan.FromMinutes(_settings.LockoutMinutes));
        }
    }

    public async Task<TimeSpan?> GetLockoutRemainingAsync(string username, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        var key = Key(username);

        var value = await db.StringGetAsync(key);
        if (!value.HasValue || !int.TryParse(value.ToString(), out var count) || count < _settings.MaxFailedAttempts)
        {
            return null;
        }

        var ttl = await db.KeyTimeToLiveAsync(key);
        return ttl ?? TimeSpan.FromMinutes(_settings.LockoutMinutes);
    }

    public Task ResetAsync(string username, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        return db.KeyDeleteAsync(Key(username));
    }

    private static string Key(string username) => KeyPrefix + username.Trim().ToLowerInvariant();
}
