using System.Net.Http.Headers;

namespace Infrastructure.IntegrationTests;

public static class CookieHelper
{
    public static string ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        var setCookie = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(v => v.StartsWith($"{cookieName}=", StringComparison.OrdinalIgnoreCase))
            : null;

        if (setCookie is null)
        {
            throw new InvalidOperationException($"Cookie '{cookieName}' was not set on the response.");
        }

        var valuePart = setCookie.Split(';')[0];
        return valuePart[(cookieName.Length + 1)..];
    }

    public static void AttachCookie(HttpRequestMessage request, string cookieName, string value)
    {
        request.Headers.TryAddWithoutValidation("Cookie", $"{cookieName}={value}");
    }

    public static void AttachBearerToken(HttpRequestMessage request, string accessToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
