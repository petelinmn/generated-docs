// Example: a write-ahead log: replay after a crash, stop at a torn record.
// Module: system-design/04-data-storage (expert)
// Tests: dotnet test --filter "FullyQualifiedName~WriteAhead"
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace DataStorage.Examples;

// Record: 4 bytes of length, 4 bytes of checksum, payload "key=value" (the key has no '=').
// A real log also forces the data to disk (FileStream.Flush(flushToDisk: true)) before acknowledging.
public static class WriteAheadLog
{
    public static void Append(Stream log, string key, string value)
    {
        byte[] payload = Encoding.UTF8.GetBytes($"{key}={value}");
        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteInt32LittleEndian(header, payload.Length);
        SHA256.HashData(payload).AsSpan(0, 4).CopyTo(header[4..]);

        log.Write(header);
        log.Write(payload);
    }

    public static Dictionary<string, string> Recover(Stream log)
    {
        var state = new Dictionary<string, string>();
        Span<byte> header = stackalloc byte[8];

        while (log.ReadAtLeast(header, 8, throwOnEndOfStream: false) == 8)
        {
            int length = BinaryPrimitives.ReadInt32LittleEndian(header);
            if (length < 0 || length > log.Length - log.Position) break; // cut off mid-record

            byte[] payload = new byte[length];
            log.ReadExactly(payload);
            if (!SHA256.HashData(payload).AsSpan(0, 4).SequenceEqual(header[4..])) break; // damaged

            string[] parts = Encoding.UTF8.GetString(payload).Split('=', 2);
            state[parts[0]] = parts[1];
        }
        return state;
    }
}
