using ApiDesign.Solutions;

public class SolutionsTests
{
    [Theory]
    [InlineData("get", false, true)]
    [InlineData("DELETE", false, true)]
    [InlineData("POST", false, false)]
    [InlineData("PATCH", false, false)]
    [InlineData("POST", true, true)]
    public void Retry_safety(string method, bool key, bool expected) =>
        Assert.Equal(expected, Exercises.IsRetrySafe(method, key));

    [Theory]
    [InlineData("NDI", true, 42)]
    [InlineData("###", false, 0)]
    [InlineData("YWJj", false, 0)]
    [InlineData(null, false, 0)]
    public void Cursor(string? cursor, bool ok, long expected)
    {
        Assert.Equal(ok, Exercises.TryDecodeCursor(cursor, out long id));
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData("*", "\"1\"", true)]
    [InlineData("\"1\", \"2\"", "\"2\"", true)]
    [InlineData("\"1\"", "\"2\"", false)]
    [InlineData("W/\"1\"", "W/\"1\"", false)]
    [InlineData("*", null, false)]
    public void If_match(string header, string? current, bool expected) =>
        Assert.Equal(expected, Exercises.IfMatchPasses(header, current));
}
