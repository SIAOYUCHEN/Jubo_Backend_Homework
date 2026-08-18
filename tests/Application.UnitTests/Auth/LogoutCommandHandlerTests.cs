using Application.Auth.Commands.Logout;
using Application.Common.Interfaces;
using Moq;

namespace Application.UnitTests.Auth;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRefreshToken_RevokesItsJti()
    {
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken("some-refresh-token"))
            .Returns(new RefreshTokenValidationResult(Guid.NewGuid(), "some-jti"));
        var refreshStore = new Mock<IRefreshTokenStore>();
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        var handler = new LogoutCommandHandler(jwtService.Object, refreshStore.Object, tokenBlacklist.Object);

        await handler.Handle(new LogoutCommand("some-refresh-token", null), CancellationToken.None);

        refreshStore.Verify(s => s.RevokeAsync("some-jti", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidAccessToken_BlacklistsItsJtiUntilItsExpiry()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(30);
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ParseAccessToken("some-access-token"))
            .Returns(new AccessTokenInfo("access-jti", expiresAt));
        var refreshStore = new Mock<IRefreshTokenStore>();
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        var handler = new LogoutCommandHandler(jwtService.Object, refreshStore.Object, tokenBlacklist.Object);

        await handler.Handle(new LogoutCommand(null, "some-access-token"), CancellationToken.None);

        tokenBlacklist.Verify(b => b.BlacklistAsync("access-jti", expiresAt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidTokens_DoesNothing()
    {
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken(It.IsAny<string>())).Returns((RefreshTokenValidationResult?)null);
        jwtService.Setup(s => s.ParseAccessToken(It.IsAny<string>())).Returns((AccessTokenInfo?)null);
        var refreshStore = new Mock<IRefreshTokenStore>();
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        var handler = new LogoutCommandHandler(jwtService.Object, refreshStore.Object, tokenBlacklist.Object);

        await handler.Handle(new LogoutCommand("garbage", "garbage"), CancellationToken.None);

        refreshStore.Verify(s => s.RevokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        tokenBlacklist.Verify(b => b.BlacklistAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
