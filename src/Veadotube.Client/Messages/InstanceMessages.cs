using System.Text.Json.Serialization;

namespace Veadotube.Client.Messages;

/// <summary>Request to the <c>instance</c> channel for the instance's <c>info</c> payload.</summary>
public sealed record InstanceInfoRequest
{
    /// <summary>Always <c>"info"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "info";
}

/// <summary>
/// Response payload from the <c>instance</c> channel. Sent automatically on connect and
/// in reply to <see cref="InstanceInfoRequest"/>. Mirrors the discovery file shape minus
/// the Unix timestamp, plus an <c>event</c> field of <c>"info"</c>.
/// </summary>
public sealed record InstanceInfoResponse
{
    /// <summary>Always <c>"info"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "info";

    /// <summary>Instance display name (e.g. <c>"veadotube mini"</c>).</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>Unique instance id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;

    /// <summary>veadotube version string.</summary>
    [JsonPropertyName("version")] public string Version { get; init; } = string.Empty;

    /// <summary>UI locale code (e.g. <c>"en"</c>).</summary>
    [JsonPropertyName("language")] public string Language { get; init; } = string.Empty;

    /// <summary>Server endpoint (<c>host:port</c>).</summary>
    [JsonPropertyName("server")] public string Server { get; init; } = string.Empty;
}
