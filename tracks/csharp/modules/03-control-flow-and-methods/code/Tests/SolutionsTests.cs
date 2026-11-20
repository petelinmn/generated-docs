using Solutions;

namespace Tests;

public class S01_GradesTests
{
    [Theory]
    [InlineData(100, "A")]
    [InlineData(90, "A")]
    [InlineData(89, "B")]
    [InlineData(70, "C")]
    [InlineData(60, "D")]
    [InlineData(59, "F")]
    [InlineData(0, "F")]
    public void Grade_MapsScoreRanges(int score, string expected) =>
        Assert.Equal(expected, Grades.Grade(score));

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Grade_RejectsOutOfRange(int score) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Grades.Grade(score));
}

public class S02_CoordinatesTests
{
    [Fact]
    public void TryParse_ReadsTwoIntegers()
    {
        Assert.True(Coordinates.TryParse("3,-4", out int x, out int y));
        Assert.Equal((3, -4), (x, y));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("3")]
    [InlineData("3,4,5")]
    [InlineData("a,4")]
    public void TryParse_RejectsBadInput(string? input) =>
        Assert.False(Coordinates.TryParse(input, out _, out _));
}

public class S03_PrimesTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(10, 4)]
    [InlineData(100, 25)]
    public void CountUpTo_CountsPrimes(int limit, int expected) =>
        Assert.Equal(expected, Primes.CountUpTo(limit));
}

public class S04_CountdownTests
{
    [Fact]
    public void Countdown_YieldsFromNDownToOne()
    {
        var result = new List<int>();
        foreach (int n in new Countdown(3))
        {
            result.Add(n);
        }
        Assert.Equal([3, 2, 1], result);
    }

    [Fact]
    public void Countdown_FromZero_IsEmpty()
    {
        int count = 0;
        foreach (int _ in new Countdown(0))
        {
            count++;
        }
        Assert.Equal(0, count);
    }
}
