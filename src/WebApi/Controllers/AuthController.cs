using Application.Auth.Commands.Login;
using Application.Auth.Commands.Logout;
using Application.Auth.Commands.RefreshToken;
using Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Contracts.Auth;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Username, request.Password), cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new AccessTokenResponse(result.AccessToken));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokenResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            throw new RefreshTokenInvalidException();
        }

        var result = await _sender.Send(new RefreshTokenCommand(refreshToken), cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new AccessTokenResponse(result.AccessToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);
        var accessToken = ExtractBearerToken();

        if (!string.IsNullOrEmpty(refreshToken) || !string.IsNullOrEmpty(accessToken))
        {
            await _sender.Send(new LogoutCommand(refreshToken, accessToken), cancellationToken);
        }

        Response.Cookies.Delete(RefreshTokenCookieName);
        return NoContent();
    }

    private string? ExtractBearerToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header["Bearer ".Length..] : null;
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(1),
        });
    }
}
