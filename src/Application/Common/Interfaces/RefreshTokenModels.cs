namespace Application.Common.Interfaces;

public record RefreshTokenIssueResult(string Token, string Jti);

public record RefreshTokenValidationResult(Guid UserId, string Jti);

public record AccessTokenInfo(string Jti, DateTime ExpiresAtUtc);
