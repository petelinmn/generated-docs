// Solutions to the exercises of system-design/05-api-design.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace ApiDesign.Solutions;

public static class Exercises
{
    // Exercise 1: may the client repeat the request automatically?
    public static bool IsRetrySafe(string method, bool hasIdempotencyKey) =>
        method.ToUpperInvariant() is "GET" or "HEAD" or "OPTIONS" or "TRACE" or "PUT" or "DELETE"
        || hasIdempotencyKey;

    // Exercise 2: a cursor comes from the client, so bad input must not throw.
    public static bool TryDecodeCursor(string? cursor, out long id)
    {
        id = 0;
        if (string.IsNullOrEmpty(cursor) || !Base64Url.IsValid(cursor)) return false;

        string text = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(cursor));
        return long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id);
    }

    // Exercise 3: If-Match with "*" or a list of entity tags; weak tags never match.
    // Simplification: tags that contain commas are not supported.
    public static bool IfMatchPasses(string header, string? currentETag)
    {
        if (currentETag is null) return false;          // nothing to match
        if (header.Trim() == "*") return true;
        if (currentETag.StartsWith("W/", StringComparison.Ordinal)) return false;

        return header.Split(',', StringSplitOptions.TrimEntries).Contains(currentETag);
    }
}
