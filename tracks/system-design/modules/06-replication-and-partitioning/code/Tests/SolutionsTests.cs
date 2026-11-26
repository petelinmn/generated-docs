using Replication.Solutions;

public class SolutionsTests
{
    [Theory]
    [InlineData(3, 2, 2)]
    [InlineData(5, 1, 5)]
    [InlineData(5, 5, 1)]
    public void Min_read_quorum(int n, int w, int expected) =>
        Assert.Equal(expected, Exercises.MinReadQuorum(n, w));

    [Fact]
    public void Merge_takes_the_maximum_per_node()
    {
        var merged = Exercises.MergeVectors(
            new Dictionary<string, long> { ["a"] = 2, ["b"] = 1 },
            new Dictionary<string, long> { ["b"] = 3, ["c"] = 1 });

        Assert.Equal((2L, 3L, 1L), (merged["a"], merged["b"], merged["c"]));
    }

    [Fact]
    public void Skew() => Assert.Equal(4, Exercises.Skew([0, 0, 0, 1000]), 9);
}
