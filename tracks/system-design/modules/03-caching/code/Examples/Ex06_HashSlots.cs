// Example: how Redis Cluster maps a key to one of 16384 hash slots, including hash tags.
// Module: system-design/03-caching (expert)
// Tests: dotnet test --filter "FullyQualifiedName~HashSlot"
using System.Text;

namespace CachingDesign.Examples;

public static class HashSlots
{
    public const int Count = 16_384;

    // CRC-16/XMODEM: polynomial 0x1021, zero initial value, no reflection.
    public static ushort Crc16(ReadOnlySpan<byte> data)
    {
        ushort crc = 0;
        foreach (byte b in data)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
                crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
        }
        return crc;
    }

    public static int SlotOf(string key) => Crc16(Encoding.UTF8.GetBytes(HashedPart(key))) % Count;

    // Only the text between the first '{' and the first '}' after it, if it is not empty.
    private static string HashedPart(string key)
    {
        int open = key.IndexOf('{');
        if (open < 0) return key;
        int close = key.IndexOf('}', open + 1);
        return close > open + 1 ? key[(open + 1)..close] : key;
    }
}
