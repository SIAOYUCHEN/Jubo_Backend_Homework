namespace Application.Common.Interfaces;

/// <summary>
/// Tracks consecutive failed login attempts per username and locks the account out for a
/// configured window once the threshold is reached (see AccountLockoutSettings).
/// </summary>
public interface IAccountLockoutStore
{
    /// <summary>Records a failed attempt. Starts (or continues) the lockout window.</summary>
    Task RegisterFailedAttemptAsync(string username, CancellationToken cancellationToken);

    /// <summary>Returns the remaining lockout duration if the account is currently locked, otherwise null.</summary>
    Task<TimeSpan?> GetLockoutRemainingAsync(string username, CancellationToken cancellationToken);

    /// <summary>Clears the failed-attempt count (called after a successful login).</summary>
    Task ResetAsync(string username, CancellationToken cancellationToken);
}
