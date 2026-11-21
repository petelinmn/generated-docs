// Example: raw SQL — parameterized FromSql versus string concatenation (SQL injection), and SqlQuery for scalars.
// Module: csharp/15-ef-core (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~RawSql"
using Microsoft.EntityFrameworkCore;

namespace Examples;

public static class RawSql
{
    // Safe: the interpolated value becomes a SQL parameter, not part of the SQL text.
    public static int FindByNameSafe(BloggingContext context, string name) =>
        context.Blogs.FromSql($"SELECT * FROM Blogs WHERE Name = {name}").Count();

    // Dangerous: the value is glued into the SQL text, so input can change the query itself.
#pragma warning disable EF1003 // the analyzer warns about exactly this mistake
    public static int FindByNameUnsafe(BloggingContext context, string name) =>
        context.Blogs.FromSqlRaw("SELECT * FROM Blogs WHERE Name = '" + name + "'").Count();
#pragma warning restore EF1003

    public static string SafeSql(BloggingContext context, string name) =>
        context.Blogs.FromSql($"SELECT * FROM Blogs WHERE Name = {name}").ToQueryString();

    // A scalar or an unmapped type; for a scalar the column must be called Value.
    public static int TotalLikes(BloggingContext context) =>
        context.Database.SqlQuery<int>($"SELECT SUM(Likes) AS Value FROM Posts").Single();
}
