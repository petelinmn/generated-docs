// Example: local functions, recursion with memoization, static local functions.
// Module: csharp/03-control-flow-and-methods (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~LocalFunctions"
namespace Examples;

public static class LocalFunctions
{
    // The local function Fib is visible only inside Fibonacci and can use its locals (memo).
    public static long Fibonacci(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        var memo = new Dictionary<int, long>();
        return Fib(n);

        long Fib(int k)
        {
            if (k < 2)
            {
                return k;
            }
            if (memo.TryGetValue(k, out long cached))
            {
                return cached;
            }
            long result = Fib(k - 1) + Fib(k - 2);
            memo[k] = result;
            return result;
        }
    }

    // A static local function cannot capture locals: an accidental capture becomes a compile error.
    public static int SumOfSquares(int[] values)
    {
        int total = 0;
        foreach (int value in values)
        {
            total += Square(value);
        }
        return total;

        static int Square(int x) => x * x;
    }
}
