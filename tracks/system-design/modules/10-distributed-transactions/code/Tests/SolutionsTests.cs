using Transactions.Solutions;

public class SolutionsTests
{
    [Theory]
    [InlineData(new[] { Vote.Yes, Vote.Yes }, Decision.Commit)]
    [InlineData(new[] { Vote.Yes, Vote.Unknown }, Decision.Abort)]
    [InlineData(new Vote[0], Decision.Abort)]
    public void Decide(Vote[] votes, Decision expected) => Assert.Equal(expected, Exercises.Decide(votes));

    [Fact]
    public async Task Compensation_is_retried()
    {
        int calls = 0;
        Task Flaky() => ++calls < 3 ? Task.FromException(new TimeoutException()) : Task.CompletedTask;

        Assert.Equal(3, await Exercises.CompensateAsync(Flaky, 5));
        calls = 0;
        await Assert.ThrowsAsync<TimeoutException>(() => Exercises.CompensateAsync(Flaky, 2));
    }

    [Fact]
    public void Only_pending_orders_change()
    {
        Assert.True(Exercises.CanTransition(OrderStatus.Pending, OrderStatus.Approved));
        Assert.False(Exercises.CanTransition(OrderStatus.Approved, OrderStatus.Rejected));
    }
}
