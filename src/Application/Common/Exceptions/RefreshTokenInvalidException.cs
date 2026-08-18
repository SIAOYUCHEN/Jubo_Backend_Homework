namespace Application.Common.Exceptions;

public class RefreshTokenInvalidException : Exception
{
    public RefreshTokenInvalidException()
        : base("Refresh token is missing, expired, or already revoked.")
    {
    }
}
