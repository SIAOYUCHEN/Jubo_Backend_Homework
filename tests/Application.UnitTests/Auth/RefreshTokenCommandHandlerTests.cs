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
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("new-access-token");
        jwtService.Setup(s => s.ValidateRefreshToken("old-token"))
            .Returns(new RefreshTokenValidationResult(SeedData.DemoUserId, "old-jti"));
        jwtService.Setup(s => s.GenerateRefreshToken(SeedData.DemoUserId))
            .Returns(new RefreshTokenIssueResult("new-refresh-token", "new-jti"));
        var refreshStore = new Mock<IRefreshTokenStore>();
        refreshStore.Setup(s => s.IsActiveAsync("old-jti", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new RefreshTokenCommandHandler(context, jwtService.Object, refreshStore.Object);

        var result = await handler.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        refreshStore.Verify(s => s.RevokeAsync("old-jti", It.IsAny<CancellationToken>()), Times.Once,
            "rotation must revoke the presented jti before issuing a new one");
        refreshStore.Verify(s => s.RegisterAsync("new-jti", SeedData.DemoUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidJwt_ThrowsRefreshTokenInvalid()
    {
        using var context = TestDbContextFactory.Create();
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken("bad-token")).Returns((RefreshTokenValidationResult?)null);
        var refreshStore = new Mock<IRefreshTokenStore>();

        var handler = new RefreshTokenCommandHandler(context, jwtService.Object, refreshStore.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("bad-token"), CancellationToken.None);

        await act.Should().ThrowAsync<RefreshTokenInvalidException>();
        refreshStore.Verify(s => s.RevokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidJwtButRevokedJti_ThrowsRefreshTokenInvalid()
    {
        using var context = TestDbContextFactory.Create();
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.ValidateRefreshToken("rotated-out-token"))
            .Returns(new RefreshTokenValidationResult(SeedData.DemoUserId, "revoked-jti"));
        var refreshStore = new Mock<IRefreshTokenStore>();
        refreshStore.Setup(s => s.IsActiveAsync("revoked-jti", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new RefreshTokenCommandHandler(context, jwtService.Object, refreshStore.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("rotated-out-token"), CancellationToken.None);

        await act.Should().ThrowAsync<RefreshTokenInvalidException>(
            "a structurally-valid JWT whose jti was already rotated out/revoked must still be rejected");
        refreshStore.Verify(s => s.RevokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
