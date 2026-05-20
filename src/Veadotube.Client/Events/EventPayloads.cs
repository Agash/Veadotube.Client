using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Veadotube.Client.Serialization;

namespace Veadotube.Client.Events;

/// <summary>Pushed when the node-list on the <c>nodes</c> channel changes after a listen subscription.</summary>
public sealed record NodeListChangedEventPayload : IVeadotubeEvent<NodeListChangedEventPayload>
{
    /// <inheritdoc />
    public static string EventName => "nodes.list";
    /// <inheritdoc />
    public static JsonTypeInfo<NodeListChangedEventPayload> JsonTypeInfo => VeadotubeJsonContext.Default.NodeListChangedEventPayload;

    /// <summary>The new node list.</summary>
    [JsonPropertyName("entries")] public IReadOnlyList<Messages.NodeListEntry> Entries { get; init; } = [];
}

/// <summary>Pushed for each value change on a subscribed <c>boolean</c> node.</summary>
public sealed record BooleanChangedEventPayload : IVeadotubeEvent<BooleanChangedEventPayload>
{
    /// <inheritdoc />
    public static string EventName => "boolean.get";
    /// <inheritdoc />
    public static JsonTypeInfo<BooleanChangedEventPayload> JsonTypeInfo => VeadotubeJsonContext.Default.BooleanChangedEventPayload;

    /// <summary>The current value, or <c>null</c> when the node has no value set.</summary>
    [JsonPropertyName("value")] public bool? Value { get; init; }
}

/// <summary>Pushed for each value change on a subscribed <c>number</c> node.</summary>
public sealed record NumberChangedEventPayload : IVeadotubeEvent<NumberChangedEventPayload>
{
    /// <inheritdoc />
    public static string EventName => "number.get";
    /// <inheritdoc />
    public static JsonTypeInfo<NumberChangedEventPayload> JsonTypeInfo => VeadotubeJsonContext.Default.NumberChangedEventPayload;

    /// <summary>New numeric value.</summary>
    [JsonPropertyName("value")] public double Value { get; init; }

    /// <summary>Lower bound when reported.</summary>
    [JsonPropertyName("min")] public double? Min { get; init; }

    /// <summary>Upper bound when reported.</summary>
    [JsonPropertyName("max")] public double? Max { get; init; }
}

/// <summary>Pushed for each top-of-stack change on a subscribed <c>stateEvents</c> node.</summary>
public sealed record StateChangedEventPayload : IVeadotubeEvent<StateChangedEventPayload>
{
    /// <inheritdoc />
    public static string EventName => "stateEvents.peek";
    /// <inheritdoc />
    public static JsonTypeInfo<StateChangedEventPayload> JsonTypeInfo => VeadotubeJsonContext.Default.StateChangedEventPayload;

    /// <summary>Currently-active state id; <c>null</c> when the stack is empty.</summary>
    [JsonPropertyName("state")] public string? State { get; init; }
}

/// <summary>Pushed at connect, and whenever the instance re-emits info on the <c>instance</c> channel.</summary>
public sealed record InstanceInfoEventPayload : IVeadotubeEvent<InstanceInfoEventPayload>
{
    /// <inheritdoc />
    public static string EventName => "instance.info";
    /// <inheritdoc />
    public static JsonTypeInfo<InstanceInfoEventPayload> JsonTypeInfo => VeadotubeJsonContext.Default.InstanceInfoEventPayload;

    /// <summary>Instance display name.</summary>
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    /// <summary>Unique instance id.</summary>
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;

    /// <summary>veadotube version.</summary>
    [JsonPropertyName("version")] public string Version { get; init; } = string.Empty;

    /// <summary>UI language code.</summary>
    [JsonPropertyName("language")] public string Language { get; init; } = string.Empty;

    /// <summary>Reported server endpoint (<c>host:port</c>).</summary>
    [JsonPropertyName("server")] public string Server { get; init; } = string.Empty;
}
