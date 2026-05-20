using System.IO;
using System.Text.Json;
using Veadotube.Client.Serialization;

namespace Veadotube.Client.Discovery;

/// <summary>
/// Reads the platform-appropriate <c>~/.veadotube/instances/</c> directory and yields
/// information about every running veadotube instance. Stale entries (timestamp more than
/// <see cref="StaleAfter"/> in the past) are filtered out per the official docs.
/// </summary>
public static class VeadotubeInstanceDiscovery
{
    /// <summary>Discovery files older than this are considered stale and ignored.</summary>
    public static TimeSpan StaleAfter { get; } = TimeSpan.FromSeconds(10);

    /// <summary>Resolves the per-user discovery directory path.</summary>
    public static string GetInstancesDirectory()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".veadotube", "instances");
    }

    /// <summary>Reads all live instances; returns an empty list if the directory does not exist.</summary>
    /// <param name="typeFilter">When supplied, only instances whose <see cref="VeadotubeInstanceInfo.InstanceType"/> matches are returned (e.g. <c>"mini"</c> or <c>"veado"</c>).</param>
    public static IReadOnlyList<VeadotubeInstanceInfo> Enumerate(string? typeFilter = null)
    {
        string dir = GetInstancesDirectory();
        if (!Directory.Exists(dir))
        {
            return [];
        }

        long nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long maxAge = (long)StaleAfter.TotalSeconds;
        List<VeadotubeInstanceInfo> result = [];

        foreach (string path in Directory.EnumerateFiles(dir))
        {
            VeadotubeInstanceInfo? parsed = TryReadOne(path);
            if (parsed is null) continue;
            if (nowUnixSeconds - parsed.Time > maxAge) continue;
            if (!string.IsNullOrEmpty(typeFilter) && !string.Equals(parsed.InstanceType, typeFilter, StringComparison.Ordinal)) continue;
            result.Add(parsed);
        }

        return result;
    }

    /// <summary>Returns the first live instance matching <paramref name="typeFilter"/>, or <c>null</c>.</summary>
    public static VeadotubeInstanceInfo? FindFirstAlive(string? typeFilter = null)
    {
        IReadOnlyList<VeadotubeInstanceInfo> all = Enumerate(typeFilter);
        return all.Count == 0 ? null : all[0];
    }

    private static VeadotubeInstanceInfo? TryReadOne(string path)
    {
        try
        {
            string text = File.ReadAllText(path);
            return JsonSerializer.Deserialize(text, VeadotubeJsonContext.Default.VeadotubeInstanceInfo);
        }
        catch (IOException) { return null; }
        catch (JsonException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }
}
