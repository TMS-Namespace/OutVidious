using System.IO.Hashing;
using System.Text;

namespace TMS.Apps.FrontTube.Backend.Common.ProviderCore.Tools;

/// <summary>
/// Provides helper methods for computing XxHash64 hashes for cache lookups.
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Computes a 64-bit XxHash64 hash from a string value (typically a remote URL).
    /// </summary>
    /// <param name="value">The string value to hash.</param>
    /// <returns>A 64-bit hash value.</returns>
    public static long ComputeHash(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var bytes = Encoding.UTF8.GetBytes(value);
        var hashBytes = XxHash64.Hash(bytes);
        return BitConverter.ToInt64(hashBytes);
    }

    /// <summary>
    /// Computes a 64-bit XxHash64 hash from a URI.
    /// </summary>
    /// <param name="uri">The URI to hash.</param>
    /// <returns>A 64-bit hash value.</returns>
    public static long ComputeHash(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        return ComputeHash(uri.ToString());
    }
}
