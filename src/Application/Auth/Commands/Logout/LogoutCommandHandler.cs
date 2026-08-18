using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenBlacklist _tokenBlacklist;

    public LogoutCommandHandler(IJwtTokenService jwtTokenService, ITokenBlacklist tokenBlacklist)
    {
        _jwtTokenService = jwtTokenService;
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
                await _tokenBlacklist.BlacklistAsync(validation.Jti, validation.ExpiresAtUtc, cancellationToken);
            }
        }
    }
}
