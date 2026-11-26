using DataStorage.Solutions;

public class SolutionsTests
{
    [Fact]
    public void Keys_sort_by_chat_then_newest_first()
    {
        string[] keys =
        [
            Exercises.MessageKey(2, 100),
            Exercises.MessageKey(1, 100),
            Exercises.MessageKey(1, 200),
            Exercises.MessageKey(10, 100),
        ];

        Assert.Equal([keys[2], keys[1], keys[0], keys[3]], keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Merge_keeps_or_drops_tombstones()
    {
        var older = new Dictionary<string, string?> { ["a"] = "1", ["b"] = "2" };
        var newer = new Dictionary<string, string?> { ["b"] = null, ["c"] = "3" };

        Assert.Equal(["a", "b", "c"], Exercises.Merge(older, newer, dropTombstones: false).Keys);
        Assert.Equal(["a", "c"], Exercises.Merge(older, newer, dropTombstones: true).Keys);
    }

    [Fact]
    public void Write_amplification() =>
        Assert.Equal(4, Exercises.WriteAmplification(100, 100, 100, 200), 9);
}
