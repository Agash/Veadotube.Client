using Veadotube.Client.Messages;
using Veadotube.Client.Serialization;

namespace Veadotube.Client;

/// <summary>Typed surface for a single <c>number</c> node, returned by <see cref="VeadotubeClient.Number"/>.</summary>
public sealed class NumberNodeAccessor
{
    private readonly VeadotubeClient _client;
    private readonly string _nodeId;

    internal NumberNodeAccessor(VeadotubeClient client, string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        _client = client;
        _nodeId = nodeId;
    }

    /// <summary>Reads the current value.</summary>
    public Task<NumberValueResponse> GetAsync(CancellationToken ct = default)
        => _client.RequestNodeAsync(
            VeadotubeApi.NumberNodeType, _nodeId,
            new NumberGetRequest(),
            VeadotubeJsonContext.Default.NumberGetRequest,
            VeadotubeJsonContext.Default.NumberValueResponse,
            "get", ct);

    /// <summary>Sets the value.</summary>
    public Task SetAsync(double value, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.NumberNodeType, _nodeId,
            new NumberSetRequest { Value = value },
            VeadotubeJsonContext.Default.NumberSetRequest, ct);

    /// <summary>Adds <paramref name="delta"/> (signed) to the current value.</summary>
    public Task AddAsync(double delta, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.NumberNodeType, _nodeId,
            new NumberAddRequest { Value = delta },
            VeadotubeJsonContext.Default.NumberAddRequest, ct);

    /// <summary>Clears the value back to unset.</summary>
    public Task ClearAsync(CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.NumberNodeType, _nodeId,
            new NumberClearRequest(),
            VeadotubeJsonContext.Default.NumberClearRequest, ct);

    /// <summary>Subscribes to value-change pushes.</summary>
    public Task ListenAsync(string? token = null, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.NumberNodeType, _nodeId,
            new NodeListenPayload { Event = "listen", Token = token },
            VeadotubeJsonContext.Default.NodeListenPayload, ct);

    /// <summary>Cancels a previously-issued listen.</summary>
    public Task UnlistenAsync(string? token = null, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.NumberNodeType, _nodeId,
            new NodeListenPayload { Event = "unlisten", Token = token },
            VeadotubeJsonContext.Default.NodeListenPayload, ct);
}
