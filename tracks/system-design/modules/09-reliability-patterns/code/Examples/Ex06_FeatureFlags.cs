// Example: a percentage rollout that keeps each user on the same side.
// Module: system-design/09-reliability-patterns (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Rollout"
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Reliability.Examples;

// In an application use Microsoft.FeatureManagement; this shows the idea of stable bucketing.
public static class Rollout
{
    // The flag name is hashed too, so the same users are not always the first for every flag.
    public static bool IsEnabled(string flag, string userId, int percent)
    {
        uint hash = BinaryPrimitives.ReadUInt32BigEndian(SHA256.HashData(Encoding.UTF8.GetBytes($"{flag}\n{userId}")));
        return hash % 100 < percent;
    }
}
