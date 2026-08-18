namespace Application.Common.Interfaces;

/// <summary>
/// Denylist of access-token jti's that have been explicitly revoked (e.g. on logout),
/// so they stop working immediately instead of waiting out their natural expiry.
/// </summary>
public interface ITokenBlacklist
{
    Task BlacklistAsync(string jti, DateTime expiresAtUtc, CancellationToken cancellationToken);

    Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken);
}
