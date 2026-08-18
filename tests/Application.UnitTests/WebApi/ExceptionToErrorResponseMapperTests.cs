using Application.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using WebApi.Middleware;

namespace Application.UnitTests.WebApi;

public class ExceptionToErrorResponseMapperTests
{
    [Fact]
    public void Map_ValidationException_Returns400WithFieldErrors()
    {
        var validationException = new ValidationException(new[]
        {
            new FluentValidation.Results.ValidationFailure("Name", "Name is required"),
        });

        var (statusCode, body) = ExceptionToErrorResponseMapper.Map(validationException);

        statusCode.Should().Be(StatusCodes.Status400BadRequest);
        body.ErrorCode.Should().BeNull();
        body.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public void Map_NotFoundException_Returns404()
    {
        var (statusCode, body) = ExceptionToErrorResponseMapper.Map(new NotFoundException("Patient", Guid.NewGuid()));

        statusCode.Should().Be(StatusCodes.Status404NotFound);
        body.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Map_InvalidCredentialsException_Returns401WithInvalidCredentialsCode()
    {
        var (statusCode, body) = ExceptionToErrorResponseMapper.Map(new InvalidCredentialsException());

        statusCode.Should().Be(StatusCodes.Status401Unauthorized);
        body.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public void Map_RefreshTokenInvalidException_Returns401WithRefreshInvalidCode()
    {
        var (statusCode, body) = ExceptionToErrorResponseMapper.Map(new RefreshTokenInvalidException());

        statusCode.Should().Be(StatusCodes.Status401Unauthorized);
        body.ErrorCode.Should().Be("REFRESH_INVALID");
    }

    [Fact]
    public void Map_UnexpectedException_Returns500WithGenericMessage_NoStackTraceLeaked()
    {
        var (statusCode, body) = ExceptionToErrorResponseMapper.Map(new InvalidOperationException("db connection string leaked here"));

        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        body.Message.Should().NotContain("db connection string leaked here");
        body.ErrorCode.Should().BeNull();
    }
}
