// Example: what EF can translate to SQL, client evaluation in the final projection, and IQueryable vs IEnumerable.
// Module: csharp/15-ef-core (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~Translation"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class Translation
{
    // A plain C# method: EF cannot turn it into SQL.
    public static string Shorten(string title) => title.Length <= 6 ? title : title[..6] + "…";

    // Allowed: the untranslatable call is in the top-level Select, EF runs it on the loaded rows.
    public static List<string> ClientEvalInProjection(BloggingContext context) =>
        context.Posts.OrderBy(p => p.Id).Select(p => Shorten(p.Title)).ToList();

    // Not allowed: the same call in Where cannot be executed by the database, EF throws.
    public static List<Post> ClientEvalInWhere(BloggingContext context) =>
        context.Posts.Where(p => Shorten(p.Title).EndsWith("…")).ToList();

    // IQueryable: the filter becomes part of the SQL, the database returns only matching rows.
    public static (int Result, string Sql) FilterInDatabase(BloggingContext context)
    {
        IQueryable<Post> query = context.Posts.Where(p => p.Likes > 30);
        return (query.Count(), query.ToQueryString());
    }

    // IEnumerable: AsEnumerable switches to LINQ to Objects, every post is loaded and filtered in memory.
    public static (int Result, int RowsRead) FilterInMemory(BloggingContext context)
    {
        int rowsRead = 0;
        int count = context.Posts
            .AsEnumerable()
            .Where(p => { rowsRead++; return p.Likes > 30; })
            .Count();
        return (count, rowsRead);
    }
}
