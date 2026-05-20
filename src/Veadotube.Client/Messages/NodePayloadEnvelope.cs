using System.Text.Json;
using System.Text.Json.Serialization;

namespace Veadotube.Client.Messages;

/// <summary>
/// Wrapper for any per-node message routed through the <c>nodes</c> channel. Every node
/// type (<see cref="VeadotubeApi.BooleanNodeType"/>, <see cref="VeadotubeApi.NumberNodeType"/>,
/// <see cref="VeadotubeApi.StateEventsNodeType"/>) shares this outer shape; only the inner
/// <see cref="Payload"/> document differs.
/// </summary>
public sealed record NodePayloadEnvelope
{
    /// <summary>Always <c>"payload"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "payload";

    /// <summary>Node type — one of the <see cref="VeadotubeApi"/> node-type constants.</summary>
    [JsonPropertyName("type")] public required string Type { get; init; }

    /// <summary>Target node id within the instance.</summary>
    [JsonPropertyName("id")] public required string Id { get; init; }

    /// <summary>Optional display name (echoed by the server in responses).</summary>
    [JsonPropertyName("name")] public string? Name { get; init; }

    /// <summary>The inner per-node payload. Deserialise into the node-type-specific record (e.g. <c>StateEventsPayload</c>).</summary>
    [JsonPropertyName("payload")] public JsonElement Payload { get; init; }
}

/// <summary>
/// Generic listen/unlisten payload that every node type accepts. Pass via
/// <see cref="NodePayloadEnvelope.Payload"/>.
/// </summary>
public sealed record NodeListenPayload
{
    /// <summary>One of <c>"listen"</c> or <c>"unlisten"</c>.</summary>
    [JsonPropertyName("event")] public required string Event { get; init; }

    /// <summary>Subscription token; must match between paired listen/unlisten.</summary>
    [JsonPropertyName("token")] public string? Token { get; init; }
}
