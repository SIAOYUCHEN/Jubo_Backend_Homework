using Application.Auth.Commands.Login;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Moq;

namespace Application.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        using var context = TestDbContextFactory.Create();
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        jwtService.Setup(s => s.GenerateRefreshToken(SeedData.DemoUserId))
            .Returns(new RefreshTokenIssueResult("refresh-token", "jti-1"));
        var refreshStore = new Mock<IRefreshTokenStore>();

        var handler = new LoginCommandHandler(context, jwtService.Object, refreshStore.Object);

        var result = await handler.Handle(new LoginCommand("demo", "demo"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        refreshStore.Verify(s => s.RegisterAsync("jti-1", SeedData.DemoUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentials()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new LoginCommandHandler(context, Mock.Of<IJwtTokenService>(), Mock.Of<IRefreshTokenStore>());

        var act = () => handler.Handle(new LoginCommand("demo", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_UnknownUsername_ThrowsInvalidCredentials()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new LoginCommandHandler(context, Mock.Of<IJwtTokenService>(), Mock.Of<IRefreshTokenStore>());

        var act = () => handler.Handle(new LoginCommand("nobody", "demo"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
