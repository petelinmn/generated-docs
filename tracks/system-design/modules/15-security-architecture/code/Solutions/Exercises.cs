// Solutions to the exercises of system-design/15-security-architecture.
// Tests: dotnet test --filter "FullyQualifiedName~Solutions"
namespace SecurityDesign.Solutions;

public static class Exercises
{
    private static readonly string[] Sensitive = ["Authorization", "Cookie", "Set-Cookie", "X-Api-Key"];

    // Exercise 1: hide secrets before headers reach a log.
    public static Dictionary<string, string> Redact(IReadOnlyDictionary<string, string> headers) =>
        headers.ToDictionary(
            h => h.Key,
            h => Sensitive.Contains(h.Key, StringComparer.OrdinalIgnoreCase) ? "***" : h.Value);

    // Exercise 2: an outgoing URL must be https to an exactly listed host.
    // This is one layer: DNS and redirects also have to be controlled against SSRF.
    public static bool IsAllowedTarget(string url, IReadOnlySet<string> allowedHosts) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.IsDefaultPort
        && uri.UserInfo.Length == 0
        && allowedHosts.Contains(uri.Host);

    // Exercise 3: a user-supplied name must stay inside the root directory.
    public static string? SafePath(string root, string name)
    {
        string fullRoot = Path.GetFullPath(root);
        if (!Path.EndsInDirectorySeparator(fullRoot)) fullRoot += Path.DirectorySeparatorChar;

        string full = Path.GetFullPath(Path.Combine(fullRoot, name));
        // Ordinal is the strict choice; symbolic links are not resolved here.
        return full.StartsWith(fullRoot, StringComparison.Ordinal) ? full : null;
    }
}
