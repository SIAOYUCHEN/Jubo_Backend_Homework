using Application.Auth.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenBlacklist _tokenBlacklist;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        ITokenBlacklist tokenBlacklist)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _tokenBlacklist = tokenBlacklist;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var validation = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        if (validation is null || await _tokenBlacklist.IsBlacklistedAsync(validation.Jti, cancellationToken))
        {
            throw new RefreshTokenInvalidException();
        }

        // Rotate: blacklist the presented jti (for its remaining lifetime) so it can't be reused,
        // even though the JWT itself would otherwise still validate.
        await _tokenBlacklist.BlacklistAsync(validation.Jti, validation.ExpiresAtUtc, cancellationToken);

        var user = await _context.Users.FindAsync(new object?[] { validation.UserId }, cancellationToken)
            ?? throw new RefreshTokenInvalidException();

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        return new AuthResultDto { AccessToken = accessToken, RefreshToken = newRefreshToken.Token };
    }
}
