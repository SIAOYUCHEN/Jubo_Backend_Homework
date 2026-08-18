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
        var validation = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        if (validation is null || !await _refreshTokenStore.IsActiveAsync(validation.Jti, cancellationToken))
        {
            throw new RefreshTokenInvalidException();
        }

        // Rotate: revoke the presented jti before issuing a new one.
        await _refreshTokenStore.RevokeAsync(validation.Jti, cancellationToken);

        var user = await _context.Users.FindAsync(new object?[] { validation.UserId }, cancellationToken)
            ?? throw new RefreshTokenInvalidException();

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);
        await _refreshTokenStore.RegisterAsync(newRefreshToken.Jti, user.Id, cancellationToken);

        return new AuthResultDto { AccessToken = accessToken, RefreshToken = newRefreshToken.Token };
    }
}
