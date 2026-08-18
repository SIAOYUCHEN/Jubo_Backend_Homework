using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);

    /// <summary>Parses and validates an access token's signature/issuer/audience (not its lifetime), for blacklisting on logout.</summary>
    AccessTokenInfo? ParseAccessToken(string token);

    RefreshTokenIssueResult GenerateRefreshToken(Guid userId);

    /// <summary>Validates a refresh token's signature/issuer/audience/lifetime. Returns null if invalid, tampered, or expired.</summary>
    RefreshTokenValidationResult? ValidateRefreshToken(string token);
}
