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
        var refreshStore = new Mock<IRefreshTokenStore>();
        refreshStore.Setup(s => s.GetUserIdAsync("old-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(SeedData.DemoUserId);
        refreshStore.Setup(s => s.IssueAsync(SeedData.DemoUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("new-refresh-token");

        var handler = new RefreshTokenCommandHandler(context, jwtService.Object, refreshStore.Object);

        var result = await handler.Handle(new RefreshTokenCommand("old-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        refreshStore.Verify(s => s.RevokeAsync("old-token", It.IsAny<CancellationToken>()), Times.Once,
            "rotation must revoke the presented token before issuing a new one");
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsRefreshTokenInvalid()
    {
        using var context = TestDbContextFactory.Create();
        var refreshStore = new Mock<IRefreshTokenStore>();
        refreshStore.Setup(s => s.GetUserIdAsync("bad-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var handler = new RefreshTokenCommandHandler(context, Mock.Of<IJwtTokenService>(), refreshStore.Object);

        var act = () => handler.Handle(new RefreshTokenCommand("bad-token"), CancellationToken.None);

        await act.Should().ThrowAsync<RefreshTokenInvalidException>();
        refreshStore.Verify(s => s.RevokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
