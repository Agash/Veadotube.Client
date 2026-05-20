using System.Text.Json.Serialization;

namespace Veadotube.Client.Messages;

/// <summary>Get-current-value request for a <c>boolean</c> node.</summary>
public sealed record BooleanGetRequest
{
    /// <summary>Always <c>"get"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "get";
}

/// <summary>Set-value request for a <c>boolean</c> node.</summary>
public sealed record BooleanSetRequest
{
    /// <summary>Always <c>"set"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "set";

    /// <summary>Target value.</summary>
    [JsonPropertyName("value")] public required bool Value { get; init; }
}

/// <summary>Toggle the current value of a <c>boolean</c> node.</summary>
public sealed record BooleanToggleRequest
{
    /// <summary>Always <c>"toggle"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "toggle";
}

/// <summary>Clear the node's value back to the unset state.</summary>
public sealed record BooleanClearRequest
{
    /// <summary>Always <c>"clear"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "clear";
}

/// <summary>
/// Response payload from a <c>boolean</c> node. The server uses several representations
/// for the value (<c>true</c>, <c>false</c>, <c>0</c>, <c>1</c>, or <c>{}</c> for unset);
/// this record normalises them via the deserializer-aware <see cref="Value"/>.
/// </summary>
public sealed record BooleanValueResponse
{
    /// <summary>The current value, or <c>null</c> when the server reports the node as unset.</summary>
    [JsonPropertyName("value")] public bool? Value { get; init; }
}
