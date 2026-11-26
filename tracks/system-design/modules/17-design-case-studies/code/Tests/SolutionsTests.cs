using CaseStudies.Solutions;

public class SolutionsTests
{
    [Theory]
    [InlineData("10", true, 62)]
    [InlineData("AzL8n0Y58m7", true, long.MaxValue)]
    [InlineData("AzL8n0Y58m8", false, 0)]
    [InlineData("ab-", false, 0)]
    [InlineData("", false, 0)]
    public void Decode(string code, bool ok, long expected)
    {
        Assert.Equal(ok, Exercises.TryDecode(code, out long value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData(62, 1)]
    [InlineData(63, 2)]
    [InlineData(3_500_000_000_000, 7)]
    public void Length(long count, int expected) => Assert.Equal(expected, Exercises.CodeLength(count));

    [Fact]
    public void Collisions_come_much_earlier_than_exhaustion()
    {
        double space = Math.Pow(62, 7);

        Assert.InRange(Exercises.CollisionProbability(1_000_000, space), 0.13, 0.14);
        Assert.InRange(Exercises.CollisionProbability(10_000_000, space), 0.999, 1.0);
    }
}
