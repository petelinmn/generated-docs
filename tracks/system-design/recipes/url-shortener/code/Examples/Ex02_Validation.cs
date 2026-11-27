// Example: which addresses and aliases a shortener should accept.
// Recipe: system-design/recipes/url-shortener (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Validation"
using System.Text.RegularExpressions;

namespace Shortener.Examples;

public static partial class LinkRules
{
    public const int MaxUrlLength = 2048;
    private static readonly string[] Reserved = ["links", "health", "admin", "api"];

    // Returns null when the address is acceptable, otherwise the reason.
    public static string? CheckUrl(string? url, string ownHost)
    {
        if (string.IsNullOrWhiteSpace(url)) return "empty";
        if (url.Length > MaxUrlLength) return "too long";
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return "not an absolute URL";
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return "only http and https";
        // https://bank.example@evil.example shows one host and opens another.
        if (uri.UserInfo.Length > 0) return "credentials are not allowed";
        // A short link that points to a short link can build a loop.
        if (string.Equals(uri.Host, ownHost, StringComparison.OrdinalIgnoreCase)) return "points to the shortener";
        return null;
    }

    public static bool IsValidAlias(string alias) =>
        AliasPattern().IsMatch(alias) && !Reserved.Contains(alias, StringComparer.OrdinalIgnoreCase);

    // \z, not $: $ also matches before a trailing newline.
    [GeneratedRegex(@"\A[A-Za-z0-9_-]{3,32}\z")]
    private static partial Regex AliasPattern();
}
