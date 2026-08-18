namespace Application.Common.Interfaces;

public interface IRefreshTokenStore
{
    /// <summary>Issues a new refresh token for the user and persists it. Returns the token value.</summary>
    Task<string> IssueAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Returns the owning user id if the token is present and not expired, otherwise null.</summary>
    Task<Guid?> GetUserIdAsync(string token, CancellationToken cancellationToken);

    /// <summary>Revokes a token. No-op if it doesn't exist.</summary>
    Task RevokeAsync(string token, CancellationToken cancellationToken);
}
