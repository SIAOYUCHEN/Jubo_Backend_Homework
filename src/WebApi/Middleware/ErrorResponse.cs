using System.Text.Json;

namespace WebApi.Middleware;

public record ErrorResponse(string Message, string? ErrorCode = null, IDictionary<string, string[]>? Errors = null)
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
}
