namespace Infrastructure.Auth;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Distinct audience for refresh tokens so a refresh JWT is rejected by the access-token
    /// bearer middleware (different ValidAudience) even though it shares the signing key.
    /// </summary>
    public string RefreshAudience { get; set; } = string.Empty;
}
