using SecurityDesign.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Redacts_secrets_in_any_casing()
    {
        var result = Exercises.Redact(new Dictionary<string, string> { ["authorization"] = "Bearer x", ["Accept"] = "*/*" });

        Assert.Equal(("***", "*/*"), (result["authorization"], result["Accept"]));
    }

    [Theory]
    [InlineData("https://api.example.com/v1", true)]
    [InlineData("http://api.example.com/v1", false)]
    [InlineData("https://api.example.com.evil.test/", false)]
    [InlineData("https://api.example.com@evil.test/", false)]
    [InlineData("https://api.example.com:8443/", false)]
    [InlineData("https://127.0.0.1/", false)]
    public void Allow_list(string url, bool expected) =>
        Assert.Equal(expected, Exercises.IsAllowedTarget(url, new HashSet<string> { "api.example.com" }));

    [Theory]
    [InlineData("report.txt", true)]
    [InlineData("sub/report.txt", true)]
    [InlineData("../secret.txt", false)]
    [InlineData("../data-evil/x", false)]
    public void Paths(string name, bool allowed) =>
        Assert.Equal(allowed, Exercises.SafePath(Path.Combine(Path.GetTempPath(), "data"), name) is not null);
}
