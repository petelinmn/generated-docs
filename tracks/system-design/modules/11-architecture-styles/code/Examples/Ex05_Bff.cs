// Example: a backend for frontend that aggregates and degrades.
// Module: system-design/11-architecture-styles (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Bff"
namespace Architecture.Examples;

public sealed record MobileHome(string Name, IReadOnlyList<string> Recommendations);

public static class MobileBff
{
    // The name is required; recommendations are optional and must not fail the screen.
    public static async Task<MobileHome> HomeAsync(
        Func<Task<string>> profile, Func<Task<IReadOnlyList<string>>> recommendations)
    {
        Task<string> name = profile();
        Task<IReadOnlyList<string>> extras = Optional(recommendations); // both run in parallel
        return new MobileHome(await name, [.. (await extras).Take(3)]); // a small screen needs three

        static async Task<IReadOnlyList<string>> Optional(Func<Task<IReadOnlyList<string>>> call)
        {
            try { return await call(); }
            catch (HttpRequestException) { return []; }
        }
    }
}
