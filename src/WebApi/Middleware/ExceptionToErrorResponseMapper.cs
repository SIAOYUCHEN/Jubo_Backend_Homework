using Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace WebApi.Middleware;

public static class ExceptionToErrorResponseMapper
{
    public static (int StatusCode, ErrorResponse Body) Map(Exception exception) => exception switch
    {
        ValidationException validationException => (
            StatusCodes.Status400BadRequest,
            new ErrorResponse("One or more validation failures have occurred.", Errors: validationException.Errors)),

        NotFoundException notFoundException => (
            StatusCodes.Status404NotFound,
            new ErrorResponse(notFoundException.Message)),

        InvalidCredentialsException => (
            StatusCodes.Status401Unauthorized,
            new ErrorResponse("Invalid username or password.", "INVALID_CREDENTIALS")),

        AccountLockedException accountLockedException => (
            StatusCodes.Status401Unauthorized,
            new ErrorResponse(accountLockedException.Message, "ACCOUNT_LOCKED")),

        RefreshTokenInvalidException => (
            StatusCodes.Status401Unauthorized,
            new ErrorResponse("Refresh token is missing, expired, or already revoked.", "REFRESH_INVALID")),

        _ => (
            StatusCodes.Status500InternalServerError,
            new ErrorResponse("An unexpected error occurred.")),
    };
}
