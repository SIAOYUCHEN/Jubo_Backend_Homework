namespace Infrastructure.Auth;

public class AccountLockoutSettings
{
    public const string SectionName = "AccountLockout";

    public int MaxFailedAttempts { get; set; } = 3;
    public int LockoutMinutes { get; set; } = 15;
}
