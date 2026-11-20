// Example: ref, out, in and params collections.
// Module: csharp/03-control-flow-and-methods (advanced)
// Tests: dotnet test --filter "FullyQualifiedName~ParameterModifiers"
namespace Examples;

public readonly struct Vector3D
{
    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
}

public static class ParameterModifiers
{
    // ref: the method works with the caller's variable itself, not with a copy.
    public static void Swap(ref int a, ref int b) => (a, b) = (b, a);

    // out: the method must assign the value before returning. This is the Try-pattern.
    public static bool TryDivide(int dividend, int divisor, out int quotient)
    {
        if (divisor == 0)
        {
            quotient = 0;
            return false;
        }
        quotient = dividend / divisor;
        return true;
    }

    // in: passed by reference but read-only, so a large struct is not copied.
    public static double Length(in Vector3D v) => Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);

    // params collections (C# 13): params over a span, no array allocation at the call site.
    public static int SumSpan(params ReadOnlySpan<int> values)
    {
        int sum = 0;
        foreach (int value in values)
        {
            sum += value;
        }
        return sum;
    }
}
