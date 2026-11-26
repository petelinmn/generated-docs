// Example: offset pagination drifts under inserts, keyset pagination does not.
// Module: system-design/05-api-design (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Pagination"
using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace ApiDesign.Examples;

public sealed record Post(long Id, string Title);

public static class Pagination
{
    // Newest first.
    public static IReadOnlyList<Post> ByOffset(IEnumerable<Post> posts, int offset, int size) =>
        [.. posts.OrderByDescending(p => p.Id).Skip(offset).Take(size)];

    public static (IReadOnlyList<Post> Items, string? NextCursor) AfterCursor(
        IEnumerable<Post> posts, string? cursor, int size)
    {
        long before = cursor is null ? long.MaxValue : Decode(cursor);

        // One extra row tells whether there is a next page without a COUNT.
        List<Post> page = [.. posts.Where(p => p.Id < before).OrderByDescending(p => p.Id).Take(size + 1)];

        bool hasMore = page.Count > size;
        if (hasMore) page.RemoveAt(size);
        return (page, hasMore ? Encode(page[^1].Id) : null);
    }

    // Opaque to clients, so the server can change what it holds. Not signed: validate it.
    public static string Encode(long id) =>
        Base64Url.EncodeToString(Encoding.UTF8.GetBytes(id.ToString(CultureInfo.InvariantCulture)));

    public static long Decode(string cursor) =>
        long.Parse(Encoding.UTF8.GetString(Base64Url.DecodeFromChars(cursor)), CultureInfo.InvariantCulture);
}
