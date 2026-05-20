using System.Text.Json.Serialization;

namespace Veadotube.Client.Messages;

/// <summary>Request the list of available states from a <c>stateEvents</c> node.</summary>
public sealed record StateListRequest
{
    /// <summary>Always <c>"list"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "list";
}

/// <summary>Response describing all available states.</summary>
public sealed record StateListResponse
{
    /// <summary>Always <c>"list"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "list";

    /// <summary>The states this node exposes.</summary>
    [JsonPropertyName("states")] public IReadOnlyList<StateInfo> States { get; init; } = [];
}

/// <summary>One avatar / state entry on a <c>stateEvents</c> node.</summary>
public sealed record StateInfo
{
    /// <summary>Stable state id.</summary>
    [JsonPropertyName("id")] public required string Id { get; init; }

    /// <summary>Display name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Short fingerprint of the state's thumbnail; use this as a cache key when fetching
    /// the actual image via <see cref="StateThumbRequest"/>.
    /// </summary>
    [JsonPropertyName("thumbHash")] public string? ThumbHash { get; init; }
}

/// <summary>Request a thumbnail image for a single state.</summary>
public sealed record StateThumbRequest
{
    /// <summary>Always <c>"thumb"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "thumb";

    /// <summary>Target state id.</summary>
    [JsonPropertyName("state")] public required string State { get; init; }
}

/// <summary>Server response carrying a state thumbnail.</summary>
public sealed record StateThumbResponse
{
    /// <summary>Always <c>"thumb"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "thumb";

    /// <summary>Target state id.</summary>
    [JsonPropertyName("state")] public required string State { get; init; }

    /// <summary>Cache fingerprint matching <see cref="StateInfo.ThumbHash"/>.</summary>
    [JsonPropertyName("hash")] public string? Hash { get; init; }

    /// <summary>Image width in pixels.</summary>
    [JsonPropertyName("width")] public int Width { get; init; }

    /// <summary>Image height in pixels.</summary>
    [JsonPropertyName("height")] public int Height { get; init; }

    /// <summary>Base64-encoded PNG payload (no <c>data:</c> prefix).</summary>
    [JsonPropertyName("png")] public string? Png { get; init; }
}

/// <summary>Read the state currently at the top of the stack.</summary>
public sealed record StatePeekRequest
{
    /// <summary>Always <c>"peek"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "peek";
}

/// <summary>Server response carrying the active (top-of-stack) state.</summary>
public sealed record StatePeekResponse
{
    /// <summary>Always <c>"peek"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "peek";

    /// <summary>Currently-active state id, or <c>null</c> when the stack is empty.</summary>
    [JsonPropertyName("state")] public string? State { get; init; }
}

/// <summary>Replace the stack with a single entry (<see cref="State"/>).</summary>
public sealed record StateSetRequest
{
    /// <summary>Always <c>"set"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "set";

    /// <summary>State id to set.</summary>
    [JsonPropertyName("state")] public required string State { get; init; }
}

/// <summary>Push a state onto the top of the stack.</summary>
public sealed record StatePushRequest
{
    /// <summary>Always <c>"push"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "push";

    /// <summary>State id to push.</summary>
    [JsonPropertyName("state")] public required string State { get; init; }
}

/// <summary>Pop a state from the stack.</summary>
public sealed record StatePopRequest
{
    /// <summary>Always <c>"pop"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "pop";

    /// <summary>Specific state id to remove from the stack (any position).</summary>
    [JsonPropertyName("state")] public string? State { get; init; }
}

/// <summary>Toggle a state — removes it from the stack if present, otherwise adds it.</summary>
public sealed record StateToggleRequest
{
    /// <summary>Always <c>"toggle"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "toggle";

    /// <summary>State id to toggle.</summary>
    [JsonPropertyName("state")] public required string State { get; init; }
}

/// <summary>Clear the state stack; veadotube preserves the top entry if a non-empty stack is required.</summary>
public sealed record StateClearRequest
{
    /// <summary>Always <c>"clear"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "clear";
}
