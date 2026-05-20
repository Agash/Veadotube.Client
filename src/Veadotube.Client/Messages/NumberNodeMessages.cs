using System.Text.Json.Serialization;

namespace Veadotube.Client.Messages;

/// <summary>Get-current-value request for a <c>number</c> node.</summary>
public sealed record NumberGetRequest
{
    /// <summary>Always <c>"get"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "get";
}

/// <summary>
/// Range-bound numeric value used by both responses and the optional rich form of
/// <see cref="NumberSetRequest"/> / <see cref="NumberAddRequest"/>.
/// </summary>
public sealed record NumberValue
{
    /// <summary>Numeric value.</summary>
    [JsonPropertyName("value")] public required double Value { get; init; }

    /// <summary>Optional minimum bound (inclusive).</summary>
    [JsonPropertyName("min")] public double? Min { get; init; }

    /// <summary>Optional maximum bound (inclusive).</summary>
    [JsonPropertyName("max")] public double? Max { get; init; }
}

/// <summary>Response payload from a <c>number</c> node.</summary>
public sealed record NumberValueResponse
{
    /// <summary>Current numeric value.</summary>
    [JsonPropertyName("value")] public double Value { get; init; }

    /// <summary>Minimum bound when reported.</summary>
    [JsonPropertyName("min")] public double? Min { get; init; }

    /// <summary>Maximum bound when reported.</summary>
    [JsonPropertyName("max")] public double? Max { get; init; }
}

/// <summary>Set the value of a <c>number</c> node to <see cref="Value"/>.</summary>
public sealed record NumberSetRequest
{
    /// <summary>Always <c>"set"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "set";

    /// <summary>Target value.</summary>
    [JsonPropertyName("value")] public required double Value { get; init; }
}

/// <summary>Add <see cref="Value"/> to the current value of a <c>number</c> node (use a negative value to subtract).</summary>
public sealed record NumberAddRequest
{
    /// <summary>Always <c>"add"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "add";

    /// <summary>Delta to add (signed).</summary>
    [JsonPropertyName("value")] public required double Value { get; init; }
}

/// <summary>Clear the node back to the unset state.</summary>
public sealed record NumberClearRequest
{
    /// <summary>Always <c>"clear"</c>.</summary>
    [JsonPropertyName("event")] public string Event { get; init; } = "clear";
}
