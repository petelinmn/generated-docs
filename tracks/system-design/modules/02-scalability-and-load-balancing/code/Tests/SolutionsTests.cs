using Scaling.Examples;
using Scaling.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Skips_unhealthy_nodes_evenly()
    {
        var rr = new HealthAwareRoundRobin(["a", "b", "c"], n => n != "b");
        Assert.Equal("acac", string.Concat(Enumerable.Range(0, 4).Select(_ => rr.Pick())));
    }

    [Fact]
    public void Returns_null_when_nothing_is_healthy() =>
        Assert.Null(new HealthAwareRoundRobin(["a"], _ => false).Pick());

    [Fact]
    public void Moved_fraction() =>
        Assert.Equal(0.5, Exercises.MovedFraction(["a", "b"], _ => "x", k => k == "a" ? "x" : "y"));

    [Fact]
    public void Best_node_count()
    {
        int best = Exercises.BestNodeCount(0.05, 0.001, 200);
        Assert.Equal(31, best);
        Assert.True(ScalingLaws.Universal(0.05, 0.001, best) > ScalingLaws.Universal(0.05, 0.001, best + 1));
    }
}
