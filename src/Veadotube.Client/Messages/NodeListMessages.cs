using System.Text.Json.Serialization;

namespace Veadotube.Client.Messages;

/// <summary>Request for the current node list on the <c>nodes</c> channel.</summary>
public sealed record NodeListRequest
{
    /// <summary>Always <c>"list"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "list";
}

/// <summary>
/// Subscribe to node-list changes on the <c>nodes</c> channel. The supplied
/// <see cref="Token"/> must be reused for <see cref="NodeListUnlistenRequest"/>.
/// </summary>
public sealed record NodeListListenRequest
{
    /// <summary>Always <c>"listen"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "listen";

    /// <summary>Optional subscription token; correlate with the matching unlisten.</summary>
    [JsonPropertyName("token")] public string? Token { get; init; }
}

/// <summary>Unsubscribe a previously-issued <see cref="NodeListListenRequest"/>.</summary>
public sealed record NodeListUnlistenRequest
{
    /// <summary>Always <c>"unlisten"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "unlisten";

    /// <summary>Subscription token issued earlier.</summary>
    [JsonPropertyName("token")] public string? Token { get; init; }
}

/// <summary>Server response to <see cref="NodeListRequest"/>, also pushed when the node list changes for active listeners.</summary>
public sealed record NodeListResponse
{
    /// <summary>Always <c>"list"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "list";

    /// <summary>Nodes currently exposed by the instance.</summary>
    [JsonPropertyName("entries")] public IReadOnlyList<NodeListEntry> Entries { get; init; } = [];
}

/// <summary>One node entry in a <see cref="NodeListResponse"/>.</summary>
public sealed record NodeListEntry
{
    /// <summary>Node type (e.g. <c>"boolean"</c>, <c>"number"</c>, <c>"stateEvents"</c>).</summary>
    [JsonPropertyName("type")] public required string Type { get; init; }

    /// <summary>Stable node id within the instance.</summary>
    [JsonPropertyName("id")] public required string Id { get; init; }

    /// <summary>Display name shown in veadotube's UI.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
}
