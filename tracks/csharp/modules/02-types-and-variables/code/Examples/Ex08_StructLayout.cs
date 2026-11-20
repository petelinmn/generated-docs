// Example: field alignment adds padding to structs.
// Module: csharp/02-types-and-variables (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Layout"
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Examples;

// The int must start at an offset divisible by 4, so 3 bytes of padding follow the byte.
[StructLayout(LayoutKind.Sequential)]
public struct Padded
{
    public byte A;
    public int B;
}

// Pack = 1 removes the padding (useful for binary formats, but unaligned access can be slower).
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packed
{
    public byte A;
    public int B;
}

public static class Layout
{
    public static int SizeOfPadded => Unsafe.SizeOf<Padded>();

    public static int SizeOfPacked => Unsafe.SizeOf<Packed>();

    // A variable of a reference type holds just a pointer to the object.
    public static int SizeOfReference => Unsafe.SizeOf<PointClass>();
}
