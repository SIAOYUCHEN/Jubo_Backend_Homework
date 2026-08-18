using Application.Auth.Commands.Logout;
using Application.Common.Interfaces;
using Moq;

namespace Application.UnitTests.Auth;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRefreshToken_BlacklistsItsJtiUntilItsExpiry()
    {
        var expiresAt = DateTime.UtcNow.AddDays(1);
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken("some-refresh-token"))
            .Returns(new RefreshTokenValidationResult(Guid.NewGuid(), "some-jti", expiresAt));
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        var handler = new LogoutCommandHandler(jwtService.Object, tokenBlacklist.Object);

        await handler.Handle(new LogoutCommand("some-refresh-token", null), CancellationToken.None);

        tokenBlacklist.Verify(b => b.BlacklistAsync("some-jti", expiresAt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidAccessToken_BlacklistsItsJtiUntilItsExpiry()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ParseAccessToken("some-access-token"))
            .Returns(new AccessTokenInfo("access-jti", expiresAt));
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        var handler = new LogoutCommandHandler(jwtService.Object, tokenBlacklist.Object);

        await handler.Handle(new LogoutCommand(null, "some-access-token"), CancellationToken.None);

        tokenBlacklist.Verify(b => b.BlacklistAsync("access-jti", expiresAt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidTokens_DoesNothing()
    {
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken(It.IsAny<string>())).Returns((RefreshTokenValidationResult?)null);
        jwtService.Setup(s => s.ParseAccessToken(It.IsAny<string>())).Returns((AccessTokenInfo?)null);
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        var handler = new LogoutCommandHandler(jwtService.Object, tokenBlacklist.Object);

        await handler.Handle(new LogoutCommand("garbage", "garbage"), CancellationToken.None);

        tokenBlacklist.Verify(b => b.BlacklistAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
