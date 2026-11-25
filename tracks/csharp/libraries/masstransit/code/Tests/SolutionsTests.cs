using Solutions;

namespace Tests;

public class SolutionsTests
{
    [Fact]
    public async Task Exercise1_ReturnsCustomerAndAmount()
    {
        string seen = await Exercises.FirstMessageAsync(TestContext.Current.CancellationToken);

        Assert.Equal("greeter: alice 19.99", seen);
    }

    [Fact]
    public async Task Exercise2_PublishGoesToBoth_SendGoesToOne()
    {
        var (afterPublish, afterSend) = await Exercises.PublishThenSendAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, afterPublish);
        Assert.Equal(["auditor"], afterSend);
    }
}
