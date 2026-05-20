using Veadotube.Client.Messages;
using Veadotube.Client.Serialization;

namespace Veadotube.Client;

/// <summary>Typed surface for a single <c>stateEvents</c> node, returned by <see cref="VeadotubeClient.States"/>.</summary>
public sealed class StateEventsNodeAccessor
{
    private readonly VeadotubeClient _client;
    private readonly string _nodeId;

    internal StateEventsNodeAccessor(VeadotubeClient client, string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        _client = client;
        _nodeId = nodeId;
    }

    /// <summary>Lists all states available on the node, including thumbnail hashes.</summary>
    public Task<StateListResponse> ListAsync(CancellationToken ct = default)
        => _client.RequestNodeAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StateListRequest(),
            VeadotubeJsonContext.Default.StateListRequest,
            VeadotubeJsonContext.Default.StateListResponse,
            "list", ct);

    /// <summary>Fetches a PNG thumbnail for a single state.</summary>
    public Task<StateThumbResponse> GetThumbnailAsync(string stateId, CancellationToken ct = default)
        => _client.RequestNodeAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StateThumbRequest { State = stateId },
            VeadotubeJsonContext.Default.StateThumbRequest,
            VeadotubeJsonContext.Default.StateThumbResponse,
            "thumb", ct);

    /// <summary>Reads the state currently on top of the stack.</summary>
    public Task<StatePeekResponse> PeekAsync(CancellationToken ct = default)
        => _client.RequestNodeAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StatePeekRequest(),
            VeadotubeJsonContext.Default.StatePeekRequest,
            VeadotubeJsonContext.Default.StatePeekResponse,
            "peek", ct);

    /// <summary>Replaces the stack so <paramref name="stateId"/> is the only entry.</summary>
    public Task SetAsync(string stateId, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StateSetRequest { State = stateId },
            VeadotubeJsonContext.Default.StateSetRequest, ct);

    /// <summary>Pushes a state onto the top of the stack.</summary>
    public Task PushAsync(string stateId, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StatePushRequest { State = stateId },
            VeadotubeJsonContext.Default.StatePushRequest, ct);

    /// <summary>Removes a state from the stack. When <paramref name="stateId"/> is <c>null</c> the top of the stack is popped.</summary>
    public Task PopAsync(string? stateId = null, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StatePopRequest { State = stateId },
            VeadotubeJsonContext.Default.StatePopRequest, ct);

    /// <summary>Toggles a state — removes it if present, adds it on top otherwise.</summary>
    public Task ToggleAsync(string stateId, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StateToggleRequest { State = stateId },
            VeadotubeJsonContext.Default.StateToggleRequest, ct);

    /// <summary>Clears the stack (veadotube preserves the top entry if a non-empty stack is required).</summary>
    public Task ClearAsync(CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new StateClearRequest(),
            VeadotubeJsonContext.Default.StateClearRequest, ct);

    /// <summary>Subscribes to state-change pushes for this node.</summary>
    public Task ListenAsync(string? token = null, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new NodeListenPayload { Event = "listen", Token = token },
            VeadotubeJsonContext.Default.NodeListenPayload, ct);

    /// <summary>Cancels a previously-issued listen.</summary>
    public Task UnlistenAsync(string? token = null, CancellationToken ct = default)
        => _client.SendNodePayloadAsync(
            VeadotubeApi.StateEventsNodeType, _nodeId,
            new NodeListenPayload { Event = "unlisten", Token = token },
            VeadotubeJsonContext.Default.NodeListenPayload, ct);
}
