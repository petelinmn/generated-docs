using Observability.Solutions;

public class SolutionsTests
{
    private const string TraceId = "4bf92f3577b34da6a3ce929d0e0e4736";

    [Fact]
    public void Parses_a_valid_header() =>
        Assert.Equal((TraceId, "00f067aa0ba902b7", true),
            Exercises.ParseTraceParent($"00-{TraceId}-00f067aa0ba902b7-01"));

    [Theory]
    [InlineData("00-00000000000000000000000000000000-00f067aa0ba902b7-01")]
    [InlineData("00-4BF92F3577B34DA6A3CE929D0E0E4736-00f067aa0ba902b7-01")]
    [InlineData("ff-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01")]
    [InlineData("garbage")]
    public void Rejects_invalid_headers(string header) => Assert.Null(Exercises.ParseTraceParent(header));

    [Fact]
    public void Paging_needs_both_windows()
    {
        Assert.True(Exercises.ShouldPage(20, 30));
        Assert.False(Exercises.ShouldPage(20, 1));
    }

    [Fact]
    public void Sampling_is_consistent_and_roughly_proportional()
    {
        Assert.Equal(Exercises.Sample(TraceId, 0.5), Exercises.Sample(TraceId, 0.5));

        var random = new Random(5);
        var bytes = new byte[16];
        int taken = 0;
        for (int i = 0; i < 10_000; i++)
        {
            random.NextBytes(bytes);
            if (Exercises.Sample(Convert.ToHexStringLower(bytes), 0.1)) taken++;
        }
        Assert.InRange(taken, 900, 1100);
    }
}
