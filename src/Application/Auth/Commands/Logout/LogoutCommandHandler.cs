using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly ITokenBlacklist _tokenBlacklist;

    public LogoutCommandHandler(
        IJwtTokenService jwtTokenService,
        IRefreshTokenStore refreshTokenStore,
        ITokenBlacklist tokenBlacklist)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenStore = refreshTokenStore;
        _tokenBlacklist = tokenBlacklist;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.AccessToken))
        {
            var accessTokenInfo = _jwtTokenService.ParseAccessToken(request.AccessToken);
            if (accessTokenInfo is not null)
            {
                await _tokenBlacklist.BlacklistAsync(accessTokenInfo.Jti, accessTokenInfo.ExpiresAtUtc, cancellationToken);
            }
        }

        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            var validation = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
            if (validation is not null)
            {
                await _refreshTokenStore.RevokeAsync(validation.Jti, cancellationToken);
            }
        }
    }
}
