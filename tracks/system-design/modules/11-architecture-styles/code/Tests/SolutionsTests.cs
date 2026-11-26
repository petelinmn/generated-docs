using Architecture.Solutions;

public class SolutionsTests
{
    [Theory]
    [InlineData(4, 1, 0.2)]
    [InlineData(0, 3, 1)]
    [InlineData(0, 0, 0)]
    public void Instability(int incoming, int outgoing, double expected) =>
        Assert.Equal(expected, Exercises.Instability(incoming, outgoing), 9);

    [Fact]
    public void Cycles()
    {
        Assert.False(Exercises.HasCycle(new Dictionary<string, string[]> { ["a"] = ["b", "c"], ["b"] = ["c"] }));
        Assert.True(Exercises.HasCycle(new Dictionary<string, string[]> { ["a"] = ["b"], ["b"] = ["c"], ["c"] = ["a"] }));
    }

    [Fact]
    public void Superseding()
    {
        var log = new DecisionLog();
        log.Add("Monolith");
        log.Add("Extract billing", supersedes: 1);

        Assert.Equal([2], log.Active.Select(r => r.Number));
        Assert.Throws<InvalidOperationException>(() => log.Add("Again", supersedes: 1));
    }
}
