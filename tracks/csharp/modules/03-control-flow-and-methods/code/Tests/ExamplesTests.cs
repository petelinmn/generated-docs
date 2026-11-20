using System.Runtime.CompilerServices;
using Examples;

namespace Tests;

public class Ex01_BranchingTests
{
    [Fact]
    public void IfElse_ChoosesTheSign()
    {
        Assert.Equal("positive", Branching.Sign(5));
        Assert.Equal("negative", Branching.Sign(-5));
        Assert.Equal("zero", Branching.Sign(0));
    }

    [Fact]
    public void SwitchStatement_GroupsCases()
    {
        Assert.Equal("weekend", Branching.DayType(DayOfWeek.Sunday));
        Assert.Equal("workday", Branching.DayType(DayOfWeek.Monday));
    }

    [Theory]
    [InlineData(15, "FizzBuzz")]
    [InlineData(9, "Fizz")]
    [InlineData(10, "Buzz")]
    [InlineData(7, "7")]
    public void SwitchExpression_MatchesTuples(int n, string expected) =>
        Assert.Equal(expected, Branching.FizzBuzz(n));

    [Theory]
    [InlineData(-5, "freezing")]
    [InlineData(0, "cold")]
    [InlineData(25, "warm")]
    [InlineData(30, "hot")]
    public void RelationalPatterns_ClassifyRanges(int celsius, string expected) =>
        Assert.Equal(expected, Branching.Classify(celsius));
}

public class Ex02_LoopsTests
{
    [Fact]
    public void Loops_ComputeExpectedValues()
    {
        Assert.Equal(55, Loops.SumTo(10));
        Assert.Equal(6, Loops.DigitSum(123));
        Assert.Equal(6, Loops.DigitSum(-123));
        Assert.Equal(1, Loops.CountDigits(0));
        Assert.Equal(3, Loops.CountDigits(-450));
        Assert.Equal(6, Loops.SumEvenUntilZero([2, 3, 4, 0, 100]));
    }

    [Fact]
    public void MathAbs_OfMinValue_Overflows() =>
        Assert.Throws<OverflowException>(() => Loops.DigitSum(int.MinValue));

    [Fact]
    public void LabeledBreakAndContinue_ControlTheOuterLoop()
    {
        int[][] grid = [[1, 2, 3], [4, -5, 6], [7, 8, 5]];

        Assert.Equal((1, 1), Loops.FindFirst(grid, -5));
        Assert.Equal((2, 1), Loops.FindFirst(grid, 8));
        Assert.Null(Loops.FindFirst(grid, 42));
        Assert.Equal(2, Loops.CountRowsWithoutNegatives(grid));
    }
}

public class Ex03_MethodsTests
{
    [Fact]
    public void OptionalAndNamedArguments()
    {
        Assert.Equal("Hello, Ann!", Methods.Greet("Ann"));
        Assert.Equal("Hi, Bob!", Methods.Greet(greeting: "Hi", name: "Bob"));
    }

    [Fact]
    public void Params_AcceptsAnyNumberOfArguments()
    {
        Assert.Equal(0, Methods.Sum());
        Assert.Equal(6, Methods.Sum(1, 2, 3));
        Assert.Equal(3, Methods.Sum([1, 2]));
    }

    [Fact]
    public void ExpressionBodiedMethod() =>
        Assert.Equal(Math.PI * 4, Methods.CircleArea(2));
}

public class Ex04_ParameterModifiersTests
{
    [Fact]
    public void Ref_ChangesCallerVariables()
    {
        int a = 1, b = 2;
        ParameterModifiers.Swap(ref a, ref b);
        Assert.Equal((2, 1), (a, b));
    }

    [Fact]
    public void Out_ImplementsTryPattern()
    {
        Assert.True(ParameterModifiers.TryDivide(10, 3, out int quotient));
        Assert.Equal(3, quotient);
        Assert.False(ParameterModifiers.TryDivide(10, 0, out _));
    }

    [Fact]
    public void In_AndParamsSpan()
    {
        var v = new Vector3D(2, 3, 6);
        Assert.Equal(7, ParameterModifiers.Length(in v));
        Assert.Equal(6, ParameterModifiers.SumSpan(1, 2, 3));
    }
}

public class Ex05_OverloadingTests
{
    [Fact]
    public void Compiler_PicksTheBestConversion()
    {
        Assert.Equal("int", Overloading.Describe(5));
        Assert.Equal("long", Overloading.Describe(5L));
        Assert.Equal("double", Overloading.Describe(5f));
        Assert.Equal("int", Overloading.Describe('a'));
        Assert.Equal("int", Overloading.Describe((short)5));
        Assert.Equal("object", Overloading.Describe(5m));
        Assert.Equal("string", Overloading.Describe("five"));
        Assert.Equal("string", Overloading.Describe(null));
    }
}

public class Ex06_LocalFunctionsTests
{
    [Fact]
    public void Fibonacci_UsesMemoizedLocalFunction()
    {
        Assert.Equal(0, LocalFunctions.Fibonacci(0));
        Assert.Equal(55, LocalFunctions.Fibonacci(10));
        Assert.Equal(12_586_269_025L, LocalFunctions.Fibonacci(50));
        Assert.Throws<ArgumentOutOfRangeException>(() => LocalFunctions.Fibonacci(-1));
    }

    [Fact]
    public void StaticLocalFunction() =>
        Assert.Equal(14, LocalFunctions.SumOfSquares([1, 2, 3]));
}

public class Ex07_LoopPitfallsTests
{
    [Fact]
    public void ModifyingListInForeach_Throws() =>
        Assert.Throws<InvalidOperationException>(() => LoopPitfalls.RemoveNegativesWrong([1, -2, 3]));

    [Fact]
    public void RemovingBackwards_Works()
    {
        List<int> values = [1, -2, -3, 4];
        LoopPitfalls.RemoveNegatives(values);
        Assert.Equal([1, 4], values);
    }

    [Fact]
    public void Closures_CaptureLoopVariables()
    {
        Assert.Equal([1, 2, 3], LoopPitfalls.CaptureInForeach());
        Assert.Equal([4, 4, 4], LoopPitfalls.CaptureInFor());
    }
}

public class Ex08_SwitchExhaustivenessTests
{
    [Fact]
    public void UnnamedEnumValue_ThrowsAtRunTime()
    {
        Assert.Equal("go", SwitchExhaustiveness.Action(TrafficLight.Green));
        Assert.Throws<SwitchExpressionException>(() => SwitchExhaustiveness.Action((TrafficLight)42));
        Assert.Throws<ArgumentOutOfRangeException>(() => SwitchExhaustiveness.SafeAction((TrafficLight)42));
    }
}

public class Ex09_RefReturnsTests
{
    [Fact]
    public void RefReturn_AllowsInPlaceChange()
    {
        int[] values = [1, 2, 3];

        ref int slot = ref RefReturns.FindFirst(values, 2);
        slot = 20;

        Assert.Equal([1, 20, 3], values);
    }
}

public class Ex10_EvenNumbersTests
{
    [Fact]
    public void DuckTypedEnumerable_WorksWithForeach()
    {
        var result = new List<int>();
        foreach (int n in new EvenNumbers(7))
        {
            result.Add(n);
        }
        Assert.Equal([0, 2, 4, 6], result);
    }
}
