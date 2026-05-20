using System.Text.Json.Serialization;

namespace Veadotube.Client.Discovery;

/// <summary>
/// Discovery record for a running veadotube instance. Mirrors the JSON payload veadotube
/// writes into <c>~/.veadotube/instances/</c> on startup. Per the official docs:
/// <list type="bullet">
/// <item>The file name is the instance id (e.g. <c>mini-08dd3cd72f015bb9-000681a2</c>).</item>
/// <item>Instances whose <see cref="Time"/> is older than 10 seconds compared to now should be ignored — they belong to a crashed process.</item>
/// </list>
/// </summary>
public sealed record VeadotubeInstanceInfo
{
    /// <summary>Unix-seconds timestamp of the most recent heartbeat; used to filter stale entries.</summary>
    [JsonPropertyName("time")]
    public long Time { get; init; }

    /// <summary>Human-readable instance name (e.g. <c>"veadotube mini"</c>).</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Unique instance id, also used as the discovery file name.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Application version (e.g. <c>"2.1"</c>).</summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>Reported UI language code.</summary>
    [JsonPropertyName("language")]
    public string Language { get; init; } = string.Empty;

    /// <summary>WebSocket server <c>host:port</c> (e.g. <c>"127.0.0.1:2424"</c>).</summary>
    [JsonPropertyName("server")]
    public string Server { get; init; } = string.Empty;

    /// <summary>
    /// Instance type prefix parsed from <see cref="Id"/> (e.g. <c>"mini"</c> or <c>"veado"</c>),
    /// or <c>null</c> if the id is malformed.
    /// </summary>
    public string? InstanceType
    {
        get
        {
            int idx = Id.IndexOf('-', StringComparison.Ordinal);
            return idx > 0 ? Id[..idx] : null;
        }
    }

    /// <summary>Builds the WebSocket URL for this instance, optionally tagging it with a client name.</summary>
    /// <param name="clientName">Optional name surfaced to veadotube as the connecting client (via the <c>?n=</c> query parameter).</param>
    public Uri WebSocketEndpoint(string? clientName = null)
    {
        string baseUrl = $"ws://{Server}";
        if (!string.IsNullOrWhiteSpace(clientName))
        {
            baseUrl += $"?{VeadotubeApi.NameQueryParameter}={Uri.EscapeDataString(clientName)}";
        }
        return new Uri(baseUrl);
    }
}
