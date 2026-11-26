using Consensus.Solutions;

public class SolutionsTests
{
    [Theory]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 2)]
    public void Failures(int size, int expected) => Assert.Equal(expected, Exercises.ToleratedFailures(size));

    [Fact]
    public void Timeout_stays_in_range()
    {
        var random = new Random(3);
        for (int i = 0; i < 100; i++)
            Assert.InRange(Exercises.ElectionTimeout(
                TimeSpan.FromMilliseconds(150), TimeSpan.FromMilliseconds(300), random).TotalMilliseconds, 150, 300);
    }

    [Fact]
    public void Commit_needs_a_majority_and_the_current_term()
    {
        long[] terms = [1, 2, 4];

        Assert.Equal(0, Exercises.CommitIndex([2, 2, 2, 0, 0], terms, currentTerm: 4, committed: 0));
        Assert.Equal(3, Exercises.CommitIndex([3, 3, 3, 0, 0], terms, currentTerm: 4, committed: 0));
        Assert.Equal(0, Exercises.CommitIndex([3, 3, 0, 0, 0], terms, currentTerm: 4, committed: 0));
    }
}
