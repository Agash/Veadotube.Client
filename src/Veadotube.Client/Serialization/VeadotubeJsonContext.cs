using System.Text.Json;
using System.Text.Json.Serialization;
using Veadotube.Client.Discovery;
using Veadotube.Client.Events;
using Veadotube.Client.Messages;

namespace Veadotube.Client.Serialization;

/// <summary>Source-generated <see cref="JsonSerializerContext"/> covering every type the library serialises.</summary>
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(VeadotubeInstanceInfo))]
[JsonSerializable(typeof(InstanceInfoRequest))]
[JsonSerializable(typeof(InstanceInfoResponse))]
[JsonSerializable(typeof(NodeListRequest))]
[JsonSerializable(typeof(NodeListListenRequest))]
[JsonSerializable(typeof(NodeListUnlistenRequest))]
[JsonSerializable(typeof(NodeListResponse))]
[JsonSerializable(typeof(NodeListEntry))]
[JsonSerializable(typeof(NodePayloadEnvelope))]
[JsonSerializable(typeof(NodeListenPayload))]
[JsonSerializable(typeof(BooleanGetRequest))]
[JsonSerializable(typeof(BooleanSetRequest))]
[JsonSerializable(typeof(BooleanToggleRequest))]
[JsonSerializable(typeof(BooleanClearRequest))]
[JsonSerializable(typeof(BooleanValueResponse))]
[JsonSerializable(typeof(NumberGetRequest))]
[JsonSerializable(typeof(NumberSetRequest))]
[JsonSerializable(typeof(NumberAddRequest))]
[JsonSerializable(typeof(NumberClearRequest))]
[JsonSerializable(typeof(NumberValue))]
[JsonSerializable(typeof(NumberValueResponse))]
[JsonSerializable(typeof(StateListRequest))]
[JsonSerializable(typeof(StateListResponse))]
[JsonSerializable(typeof(StateInfo))]
[JsonSerializable(typeof(StateThumbRequest))]
[JsonSerializable(typeof(StateThumbResponse))]
[JsonSerializable(typeof(StatePeekRequest))]
[JsonSerializable(typeof(StatePeekResponse))]
[JsonSerializable(typeof(StateSetRequest))]
[JsonSerializable(typeof(StatePushRequest))]
[JsonSerializable(typeof(StatePopRequest))]
[JsonSerializable(typeof(StateToggleRequest))]
[JsonSerializable(typeof(StateClearRequest))]
[JsonSerializable(typeof(NodeListChangedEventPayload))]
[JsonSerializable(typeof(BooleanChangedEventPayload))]
[JsonSerializable(typeof(NumberChangedEventPayload))]
[JsonSerializable(typeof(StateChangedEventPayload))]
[JsonSerializable(typeof(InstanceInfoEventPayload))]
[JsonSerializable(typeof(NodeChannelMessage))]
[JsonSerializable(typeof(InstanceChannelMessage))]
public sealed partial class VeadotubeJsonContext : JsonSerializerContext;

/// <summary>
/// Top-level wrapper for any message sent or received on the <c>nodes</c> channel —
/// i.e. <c>{"nodes": &lt;json&gt;}</c>.
/// </summary>
public sealed record NodeChannelMessage
{
    /// <summary>Payload sent over the <c>nodes</c> channel.</summary>
    [JsonPropertyName(VeadotubeApi.NodesChannel)] public JsonElement Nodes { get; init; }
}

/// <summary>
/// Top-level wrapper for any message sent or received on the <c>instance</c> channel —
/// i.e. <c>{"instance": &lt;json&gt;}</c>.
/// </summary>
public sealed record InstanceChannelMessage
{
    /// <summary>Payload sent over the <c>instance</c> channel.</summary>
    [JsonPropertyName(VeadotubeApi.InstanceChannel)] public JsonElement Instance { get; init; }
}
