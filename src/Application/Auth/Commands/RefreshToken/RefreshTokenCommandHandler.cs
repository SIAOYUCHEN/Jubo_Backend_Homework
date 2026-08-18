using Application.Auth.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IRefreshTokenStore refreshTokenStore)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = await _refreshTokenStore.GetUserIdAsync(request.RefreshToken, cancellationToken);
        if (userId is null)
        {
            throw new RefreshTokenInvalidException();
        }

        // Rotate: revoke the presented token before issuing a new one.
        await _refreshTokenStore.RevokeAsync(request.RefreshToken, cancellationToken);

        var user = await _context.Users.FindAsync(new object?[] { userId.Value }, cancellationToken)
            ?? throw new RefreshTokenInvalidException();

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = await _refreshTokenStore.IssueAsync(user.Id, cancellationToken);

        return new AuthResultDto { AccessToken = accessToken, RefreshToken = newRefreshToken };
    }
}
