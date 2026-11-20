// Example: a method can return a reference to a storage location, not just a value.
// Module: csharp/03-control-flow-and-methods (expert)
// Tests: dotnet test --filter "FullyQualifiedName~RefReturns"
namespace Examples;

public static class RefReturns
{
    // Returns a reference to the array element itself, so the caller can change it in place.
    public static ref int FindFirst(int[] values, int target)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == target)
            {
                return ref values[i];
            }
        }
        throw new InvalidOperationException($"{target} not found");
    }
}
