using Application.Auth.Commands.Logout;
using Application.Common.Interfaces;
using Moq;

namespace Application.UnitTests.Auth;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_RevokesTheGivenToken()
    {
        var refreshStore = new Mock<IRefreshTokenStore>();
        var handler = new LogoutCommandHandler(refreshStore.Object);

        await handler.Handle(new LogoutCommand("some-token"), CancellationToken.None);

        refreshStore.Verify(s => s.RevokeAsync("some-token", It.IsAny<CancellationToken>()), Times.Once);
    }
}
