// Example: geohash: nearby points usually share a prefix, so a text index can find neighbours.
// Module: system-design/17-design-case-studies (expert)
// Tests: dotnet test --filter "FullyQualifiedName~Geohash"
namespace CaseStudies.Examples;

public static class Geohash
{
    private const string Alphabet = "0123456789bcdefghjkmnpqrstuvwxyz";

    public static string Encode(double latitude, double longitude, int length)
    {
        double south = -90, north = 90, west = -180, east = 180;
        var result = new char[length];
        bool evenBit = true; // bits alternate: longitude first

        for (int i = 0; i < length; i++)
        {
            int value = 0;
            for (int bit = 0; bit < 5; bit++, evenBit = !evenBit)
            {
                value <<= 1;
                if (evenBit)
                {
                    double middle = (west + east) / 2;
                    if (longitude >= middle) { value |= 1; west = middle; } else east = middle;
                }
                else
                {
                    double middle = (south + north) / 2;
                    if (latitude >= middle) { value |= 1; south = middle; } else north = middle;
                }
            }
            result[i] = Alphabet[value];
        }
        return new string(result);
    }
}
