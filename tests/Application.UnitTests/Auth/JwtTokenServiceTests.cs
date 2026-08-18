using System.IdentityModel.Tokens.Jwt;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Auth;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_IncludesUserClaims_AndExpiresInConfiguredWindow()
    {
        var settings = new JwtSettings
        {
            SecretKey = "unit-test-secret-key-with-enough-length-1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpiryMinutes = 60,
        };
        var service = new JwtTokenService(Options.Create(settings));
        var user = new User { Id = Guid.NewGuid(), Username = "demo", PasswordHash = "hash" };

        var token = service.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
    }
}
