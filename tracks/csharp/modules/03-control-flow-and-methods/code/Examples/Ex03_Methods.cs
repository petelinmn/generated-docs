// Example: method declarations, optional and named arguments, params, expression bodies.
// Module: csharp/03-control-flow-and-methods (basics)
// Tests: dotnet test --filter "FullyQualifiedName~Methods"
namespace Examples;

public static class Methods
{
    // An optional parameter: callers may omit it, the default value is compiled into the call site.
    public static string Greet(string name, string greeting = "Hello") => $"{greeting}, {name}!";

    // params: the caller passes any number of arguments, the method receives an array.
    public static int Sum(params int[] values)
    {
        int sum = 0;
        foreach (int value in values)
        {
            sum += value;
        }
        return sum;
    }

    // An expression-bodied method: a single expression instead of a block with return.
    public static double CircleArea(double radius) => Math.PI * radius * radius;
}
