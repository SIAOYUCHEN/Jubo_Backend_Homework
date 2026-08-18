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
    private static Mock<IAccountLockoutStore> NotLockedOutStore()
    {
        var store = new Mock<IAccountLockoutStore>();
        store.Setup(s => s.GetLockoutRemainingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeSpan?)null);
        return store;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndResetsFailureCount()
    {
        using var context = TestDbContextFactory.Create();
        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        jwtService.Setup(s => s.GenerateRefreshToken(SeedData.DemoUserId))
            .Returns(new RefreshTokenIssueResult("refresh-token", "jti-1"));
        var lockoutStore = NotLockedOutStore();

        var handler = new LoginCommandHandler(context, jwtService.Object, lockoutStore.Object);

        var result = await handler.Handle(new LoginCommand("demo", "demo"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        lockoutStore.Verify(s => s.ResetAsync("demo", It.IsAny<CancellationToken>()), Times.Once);
        lockoutStore.Verify(s => s.RegisterFailedAttemptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentials_AndRegistersFailedAttempt()
    {
        using var context = TestDbContextFactory.Create();
        var lockoutStore = NotLockedOutStore();
        var handler = new LoginCommandHandler(context, Mock.Of<IJwtTokenService>(), lockoutStore.Object);

        var act = () => handler.Handle(new LoginCommand("demo", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        lockoutStore.Verify(s => s.RegisterFailedAttemptAsync("demo", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownUsername_ThrowsInvalidCredentials_AndRegistersFailedAttempt()
    {
        using var context = TestDbContextFactory.Create();
        var lockoutStore = NotLockedOutStore();
        var handler = new LoginCommandHandler(context, Mock.Of<IJwtTokenService>(), lockoutStore.Object);

        var act = () => handler.Handle(new LoginCommand("nobody", "demo"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        lockoutStore.Verify(s => s.RegisterFailedAttemptAsync("nobody", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AccountLockedOut_ThrowsAccountLocked_WithoutCheckingPassword()
    {
        using var context = TestDbContextFactory.Create();
        var lockoutStore = new Mock<IAccountLockoutStore>();
        lockoutStore.Setup(s => s.GetLockoutRemainingAsync("demo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TimeSpan.FromMinutes(10));
        var jwtService = new Mock<IJwtTokenService>();

        var handler = new LoginCommandHandler(context, jwtService.Object, lockoutStore.Object);

        // even with the correct password, a locked-out account must be rejected outright
        var act = () => handler.Handle(new LoginCommand("demo", "demo"), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<AccountLockedException>();
        exception.Which.RemainingSeconds.Should().BeGreaterThan(0);
        jwtService.Verify(s => s.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        lockoutStore.Verify(s => s.RegisterFailedAttemptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
