using Reliability.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Backoff_stays_under_the_cap()
    {
        var random = new Random(7);
        for (int retry = 0; retry < 10; retry++)
        {
            TimeSpan delay = Exercises.Backoff(retry, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1), random);
            Assert.InRange(delay.TotalMilliseconds, 0, Math.Min(1000, 100 * Math.Pow(2, retry)));
        }
    }

    [Fact]
    public void Deadline()
    {
        var now = DateTimeOffset.UnixEpoch;

        Assert.Equal(TimeSpan.FromSeconds(2), Exercises.Remaining(now.AddSeconds(2), now, TimeSpan.FromSeconds(1)));
        Assert.Null(Exercises.Remaining(now.AddMilliseconds(500), now, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Budget_allows_one_retry_per_ten_requests()
    {
        var budget = new RetryBudget(ratio: 0.1, maxTokens: 10);
        Assert.False(budget.TryRetry());

        for (int i = 0; i < 10; i++) budget.OnRequest();

        Assert.True(budget.TryRetry());
        Assert.False(budget.TryRetry());
    }
}
