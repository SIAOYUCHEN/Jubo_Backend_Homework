namespace Application.Common.Interfaces;

/// <summary>
/// Registry of currently-active refresh token jti's (allowlist). A refresh JWT is only
/// honored while its jti is registered here — rotation/logout revoke by removing it.
/// </summary>
public interface IRefreshTokenStore
{
    Task RegisterAsync(string jti, Guid userId, CancellationToken cancellationToken);

    Task<bool> IsActiveAsync(string jti, CancellationToken cancellationToken);

    /// <summary>Revokes a jti. No-op if it doesn't exist.</summary>
    Task RevokeAsync(string jti, CancellationToken cancellationToken);
}
