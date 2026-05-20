using Veadotube.Client.Messages;
using Veadotube.Client.Serialization;

namespace Veadotube.Client;

/// <summary>Typed surface for a single <c>boolean</c> node, returned by <see cref="VeadotubeClient.Boolean"/>.</summary>
public sealed class BooleanNodeAccessor
{
    private readonly VeadotubeClient _client;
    private readonly string _nodeId;

    internal BooleanNodeAccessor(VeadotubeClient client, string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        _client = client;
        _nodeId = nodeId;
    }

    /// <summary>Reads the node's current value.</summary>
    public Task<BooleanValueResponse> GetAsync(CancellationToken ct = default)
        => _client.RequestNodeAsync(
            VeadotubeApi.BooleanNodeType, _nodeId,
            new BooleanGetRequest(),
            VeadotubeJsonContext.Default.BooleanGetRequest,
            VeadotubeJsonContext.Default.BooleanValueResponse,
            "get", ct);

    /// <summary>Sets the node's value.</summary>
    public Task SetAsync(bool value, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.BooleanNodeType, _nodeId,
            new BooleanSetRequest { Value = value },
            VeadotubeJsonContext.Default.BooleanSetRequest, ct);

    /// <summary>Inverts the node's value.</summary>
    public Task ToggleAsync(CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.BooleanNodeType, _nodeId,
            new BooleanToggleRequest(),
            VeadotubeJsonContext.Default.BooleanToggleRequest, ct);

    /// <summary>Clears the node back to the unset state.</summary>
    public Task ClearAsync(CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.BooleanNodeType, _nodeId,
            new BooleanClearRequest(),
            VeadotubeJsonContext.Default.BooleanClearRequest, ct);

    /// <summary>Subscribes to value-change pushes (the matching unlisten uses the same <paramref name="token"/>).</summary>
    public Task ListenAsync(string? token = null, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.BooleanNodeType, _nodeId,
            new NodeListenPayload { Event = "listen", Token = token },
            VeadotubeJsonContext.Default.NodeListenPayload, ct);

    /// <summary>Cancels a previously-issued listen.</summary>
    public Task UnlistenAsync(string? token = null, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.BooleanNodeType, _nodeId,
            new NodeListenPayload { Event = "unlisten", Token = token },
            VeadotubeJsonContext.Default.NodeListenPayload, ct);
}
