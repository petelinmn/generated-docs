using Consistency.Solutions;

public class SolutionsTests
{
    [Theory]
    [InlineData(2, 3, true)]
    [InlineData(2, 4, false)]
    [InlineData(3, 5, true)]
    public void Quorum(int reachable, int n, bool expected) =>
        Assert.Equal(expected, Exercises.HasQuorum(reachable, n));

    [Fact]
    public void Retry_returns_the_successful_attempt()
    {
        int calls = 0;
        Assert.Equal(3, Exercises.RunWithRetry(() => ++calls == 3, 5));
        Assert.Throws<InvalidOperationException>(() => Exercises.RunWithRetry(() => false, 2));
    }

    [Fact]
    public void Counter_converges()
    {
        var a = new GCounter("a");
        var b = new GCounter("b");
        a.Increment();
        b.Increment();
        b.Increment();

        a.Merge(b);
        a.Merge(b);
        b.Merge(a);

        Assert.Equal((3L, 3L), (a.Value, b.Value));
    }
}
