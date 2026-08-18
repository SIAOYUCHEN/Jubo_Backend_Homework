using Application.Auth.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAccountLockoutStore _accountLockoutStore;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IAccountLockoutStore accountLockoutStore)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _accountLockoutStore = accountLockoutStore;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var lockoutRemaining = await _accountLockoutStore.GetLockoutRemainingAsync(request.Username, cancellationToken);
        if (lockoutRemaining is not null)
        {
            throw new AccountLockedException(lockoutRemaining.Value);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _accountLockoutStore.RegisterFailedAttemptAsync(request.Username, cancellationToken);
            throw new InvalidCredentialsException();
        }

        await _accountLockoutStore.ResetAsync(request.Username, cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        return new AuthResultDto { AccessToken = accessToken, RefreshToken = refreshToken.Token };
    }
}
