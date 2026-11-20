using System.Runtime.CompilerServices;
using Solutions;

namespace Tests;

public class S01_TemperatureTests
{
    [Theory]
    [InlineData("36.6", 36.6)]
    [InlineData("-40", -40.0)]
    [InlineData(" 100 ", 100.0)]
    public void ParseCelsius_AcceptsInvariantNumbers(string input, double expected) =>
        Assert.Equal(expected, Temperature.ParseCelsius(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("36,6")]
    public void ParseCelsius_ReturnsNullForBadInput(string? input) =>
        Assert.Null(Temperature.ParseCelsius(input));

    [Fact]
    public void ToFahrenheit_UsesTheStandardFormula()
    {
        Assert.Equal(212, Temperature.ToFahrenheit(100));
        Assert.Equal(-40, Temperature.ToFahrenheit(-40));
    }
}

public class S02_SafeMathTests
{
    [Fact]
    public void CheckedSum_ThrowsOnOverflow()
    {
        Assert.Equal(6, SafeMath.CheckedSum([1, 2, 3]));
        Assert.Throws<OverflowException>(() => SafeMath.CheckedSum([int.MaxValue, 1]));
    }

    [Fact]
    public void WideSum_DoesNotOverflow() =>
        Assert.Equal(2L * int.MaxValue, SafeMath.WideSum([int.MaxValue, int.MaxValue]));
}

public class S03_NamesTests
{
    [Theory]
    [InlineData(null, "Anonymous")]
    [InlineData("   ", "Anonymous")]
    [InlineData("  alice ", "Alice")]
    [InlineData("bOB", "Bob")]
    public void Normalize_HandlesNullAndCasing(string? input, string expected) =>
        Assert.Equal(expected, Names.Normalize(input));
}

public class S04_StructLayoutTests
{
    [Fact]
    public void FieldOrder_ChangesStructSize()
    {
        Assert.Equal(24, Unsafe.SizeOf<Wasteful>());
        Assert.Equal(16, Unsafe.SizeOf<Compact>());
    }
}
