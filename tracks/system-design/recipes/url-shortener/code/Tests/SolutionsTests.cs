using Shortener.Solutions;

namespace Shortener.Tests;

public class SolutionsTests
{
    [Fact]
    public void Solutions_CheckCharacterCatchesTyposAndSwaps()
    {
        string code = CheckedCode.Append("Ab12");

        Assert.True(CheckedCode.IsValid(code));
        Assert.False(CheckedCode.IsValid("Ab13" + code[^1]));
        Assert.False(CheckedCode.IsValid("bA12" + code[^1]));
        Assert.False(CheckedCode.IsValid("a/b"));
    }

    [Fact]
    public void Solutions_CreationIsIdempotentPerOwner()
    {
        int next = 0;
        var creator = new IdempotentCreator(() => $"c{++next}");

        Assert.Equal("c1", creator.Create("ann", "https://example.com"));
        Assert.Equal("c1", creator.Create("ann", "https://example.com"));
        Assert.Equal("c2", creator.Create("bob", "https://example.com"));
    }

    [Fact]
    public void Solutions_KeyspaceOfSevenCharacters()
    {
        Assert.Equal(3_521_614_606_208, Keyspace.Size(7));
        Assert.InRange(Keyspace.CollisionChance(1_000_000_000, 7), 0.00028, 0.00029);
    }
}
