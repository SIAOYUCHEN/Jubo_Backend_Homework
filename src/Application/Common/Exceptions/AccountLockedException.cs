namespace Application.Common.Exceptions;

public class AccountLockedException : Exception
{
    public int RemainingSeconds { get; }

    public AccountLockedException(TimeSpan remaining)
        : base($"Account is locked due to too many failed login attempts. Try again in {SecondsOf(remaining)} seconds.")
    {
        RemainingSeconds = SecondsOf(remaining);
    }

    private static int SecondsOf(TimeSpan remaining) => (int)Math.Max(1, Math.Ceiling(remaining.TotalSeconds));
}
