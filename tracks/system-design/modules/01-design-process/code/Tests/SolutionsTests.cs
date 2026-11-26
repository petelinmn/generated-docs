using DesignProcess.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Servers() => Assert.Equal(14, Exercises.ServersNeeded(7000, 1000, 0.6, 2));

    [Theory]
    [InlineData(0.99, 0.999, 2)]
    [InlineData(0.9, 0.9995, 4)]
    [InlineData(0.999, 0.99, 1)]
    public void Replicas(double single, double target, int expected) =>
        Assert.Equal(expected, Exercises.ReplicasFor(single, target));

    [Fact]
    public void Replicas_rejects_impossible_target() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Exercises.ReplicasFor(0.9, 1.0));

    [Fact]
    public void Burn_rate() => Assert.Equal(10, Exercises.BurnRate(0.999, 10_000, 100), 6);
}
