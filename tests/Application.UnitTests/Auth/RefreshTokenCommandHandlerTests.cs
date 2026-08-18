using Application.Auth.Commands.RefreshToken;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Moq;

namespace Application.UnitTests.Auth;

public class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidToken_RotatesAndReturnsNewTokens()
    {
        using var context = TestDbContextFactory.Create();
        var expiresAt = DateTime.UtcNow.AddDays(1);
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("new-access-token");
        jwtService.Setup(s => s.ValidateRefreshToken("old-token"))
            .Returns(new RefreshTokenValidationResult(SeedData.DemoUserId, "old-jti", expiresAt));
        jwtService.Setup(s => s.GenerateRefreshToken(SeedData.DemoUserId))
            .Returns(new RefreshTokenIssueResult("new-refresh-token", "new-jti"));
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        tokenBlacklist.Setup(b => b.IsBlacklistedAsync("old-jti", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new RefreshTokenCommandHandler(context, jwtService.Object, tokenBlacklist.Object);

        var result = await handler.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        tokenBlacklist.Verify(b => b.BlacklistAsync("old-jti", expiresAt, It.IsAny<CancellationToken>()), Times.Once,
            "rotation must blacklist the presented jti so it can't be reused, even before its natural expiry");
    }

    [Fact]
    public async Task Handle_InvalidJwt_ThrowsRefreshTokenInvalid()
    {
        using var context = TestDbContextFactory.Create();
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken("bad-token")).Returns((RefreshTokenValidationResult?)null);
        var tokenBlacklist = new Mock<ITokenBlacklist>();

        var handler = new RefreshTokenCommandHandler(context, jwtService.Object, tokenBlacklist.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("bad-token"), CancellationToken.None);

        await act.Should().ThrowAsync<RefreshTokenInvalidException>();
        tokenBlacklist.Verify(b => b.BlacklistAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidJwtButAlreadyBlacklistedJti_ThrowsRefreshTokenInvalid()
    {
        using var context = TestDbContextFactory.Create();
        var expiresAt = DateTime.UtcNow.AddDays(1);
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken("rotated-out-token"))
            .Returns(new RefreshTokenValidationResult(SeedData.DemoUserId, "revoked-jti", expiresAt));
        var tokenBlacklist = new Mock<ITokenBlacklist>();
        tokenBlacklist.Setup(b => b.IsBlacklistedAsync("revoked-jti", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new RefreshTokenCommandHandler(context, jwtService.Object, tokenBlacklist.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("rotated-out-token"), CancellationToken.None);

        await act.Should().ThrowAsync<RefreshTokenInvalidException>(
            "a structurally-valid JWT whose jti was already rotated out/revoked must still be rejected");
    }
}
