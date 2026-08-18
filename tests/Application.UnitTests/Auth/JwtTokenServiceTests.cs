using System.IdentityModel.Tokens.Jwt;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Auth;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(
        int accessTokenExpiryMinutes = 60,
        int refreshTokenExpiryDays = 1)
    {
        var jwtSettings = new JwtSettings
        {
            SecretKey = "unit-test-secret-key-with-enough-length-1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            RefreshAudience = "test-audience-refresh",
            AccessTokenExpiryMinutes = accessTokenExpiryMinutes,
        };
        var refreshSettings = new RefreshTokenSettings { ExpiryDays = refreshTokenExpiryDays };
        return new JwtTokenService(Options.Create(jwtSettings), Options.Create(refreshSettings));
    }

    [Fact]
    public void GenerateAccessToken_IncludesUserClaims_AndExpiresInConfiguredWindow()
    {
        var service = CreateService(accessTokenExpiryMinutes: 60);
        var user = new User { Id = Guid.NewGuid(), Username = "demo", PasswordHash = "hash" };

        var token = service.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ParseAccessToken_ValidToken_ReturnsJtiAndExpiry()
    {
        var service = CreateService();
        var user = new User { Id = Guid.NewGuid(), Username = "demo", PasswordHash = "hash" };
        var token = service.GenerateAccessToken(user);

        var info = service.ParseAccessToken(token);

        info.Should().NotBeNull();
        info!.Jti.Should().NotBeNullOrEmpty();
        info.ExpiresAtUtc.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ParseAccessToken_GarbageInput_ReturnsNull()
    {
        var service = CreateService();

        service.ParseAccessToken("not-a-real-token").Should().BeNull();
    }

    [Fact]
    public void GenerateRefreshToken_ProducesAJwt_ThatValidatesBackToTheSameUserAndJti()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var issued = service.GenerateRefreshToken(userId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(issued.Token);

        jwt.Audiences.Should().Contain("test-audience-refresh");

        var validation = service.ValidateRefreshToken(issued.Token);

        validation.Should().NotBeNull();
        validation!.UserId.Should().Be(userId);
        validation.Jti.Should().Be(issued.Jti);
    }

    [Fact]
    public void ValidateRefreshToken_AccessTokenPresentedAsRefreshToken_IsRejected()
    {
        var service = CreateService();
        var user = new User { Id = Guid.NewGuid(), Username = "demo", PasswordHash = "hash" };
        var accessToken = service.GenerateAccessToken(user);

        service.ValidateRefreshToken(accessToken).Should().BeNull("an access token has a different audience and must not work as a refresh token");
    }

    [Fact]
    public void ValidateRefreshToken_ExpiredToken_ReturnsNull()
    {
        var service = CreateService(refreshTokenExpiryDays: 0);
        var issued = service.GenerateRefreshToken(Guid.NewGuid());

        Thread.Sleep(1100);

        service.ValidateRefreshToken(issued.Token).Should().BeNull();
    }

    [Fact]
    public void ValidateRefreshToken_TamperedToken_ReturnsNull()
    {
        var service = CreateService();
        var issued = service.GenerateRefreshToken(Guid.NewGuid());

        service.ValidateRefreshToken(issued.Token[..^2] + "xx").Should().BeNull();
    }
}
