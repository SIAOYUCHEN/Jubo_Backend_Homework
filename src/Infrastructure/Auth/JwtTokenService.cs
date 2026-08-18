using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly RefreshTokenSettings _refreshTokenSettings;

    public JwtTokenService(IOptions<JwtSettings> settings, IOptions<RefreshTokenSettings> refreshTokenSettings)
    {
        _settings = settings.Value;
        _refreshTokenSettings = refreshTokenSettings.Value;
    }

    private SymmetricSecurityKey SigningKey => new(Encoding.UTF8.GetBytes(_settings.SecretKey));

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public AccessTokenInfo? ParseAccessToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, ValidationParameters(_settings.Audience, validateLifetime: false), out var validated);

            var jwt = (JwtSecurityToken)validated;
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            return jti is null ? null : new AccessTokenInfo(jti, jwt.ValidTo);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }

    public RefreshTokenIssueResult GenerateRefreshToken(Guid userId)
    {
        var jti = Guid.NewGuid().ToString("N");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.RefreshAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_refreshTokenSettings.ExpiryDays),
            signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));

        return new RefreshTokenIssueResult(new JwtSecurityTokenHandler().WriteToken(token), jti);
    }

    public RefreshTokenValidationResult? ValidateRefreshToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(token, ValidationParameters(_settings.RefreshAudience, validateLifetime: true), out var validated);

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            return sub is not null && jti is not null && Guid.TryParse(sub, out var userId)
                ? new RefreshTokenValidationResult(userId, jti, ((JwtSecurityToken)validated).ValidTo)
                : null;
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }

    private TokenValidationParameters ValidationParameters(string audience, bool validateLifetime) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = _settings.Issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SigningKey,
        ValidateLifetime = validateLifetime,
        ClockSkew = TimeSpan.Zero,
    };
}
