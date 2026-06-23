using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Veadotube.Client.Errors;
using Veadotube.Client.Events;
using Veadotube.Client.Messages;
using Veadotube.Client.Serialization;

namespace Veadotube.Client;

/// <summary>
/// Asynchronous WebSocket client for the veadotube API. Covers the documented
/// <see cref="VeadotubeApi.InstanceChannel"/> and <see cref="VeadotubeApi.NodesChannel"/>
/// surfaces, plus typed accessors for the three built-in node types.
/// </summary>
public sealed partial class VeadotubeClient : IAsyncDisposable
{
    private readonly VeadotubeClientOptions _options;
    private readonly ILogger<VeadotubeClient> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<TaskCompletionSource<JsonElement>>> _pending = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private ClientWebSocket? _ws;
    private CancellationTokenSource? _loopCts;
    private Task? _receiveLoop;
    private bool _disposed;

    /// <summary>Initialises a new client.</summary>
    /// <param name="options">Connection and protocol options.</param>
    /// <param name="logger">Optional logger; <see cref="NullLogger{T}"/> is used when <c>null</c>.</param>
    public VeadotubeClient(VeadotubeClientOptions options, ILogger<VeadotubeClient>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _logger = logger ?? NullLogger<VeadotubeClient>.Instance;
        Events = new VeadotubeEventHub();
    }

    /// <summary>Typed event dispatcher for server-pushed messages.</summary>
    public VeadotubeEventHub Events { get; }

    /// <summary>True once the transport is open.</summary>
    public bool IsConnected => _ws is { State: WebSocketState.Open };

    /// <summary>Raised when the transport closes (planned or unexpected).</summary>
    public event EventHandler<EventArgs>? Disconnected;

    /// <summary>Opens the WebSocket connection. No-op if already connected.</summary>
    public async Task ConnectAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsConnected)
        {
            return;
        }

        _ws = new ClientWebSocket();
        try
        {
            await _ws.ConnectAsync(_options.Endpoint, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is WebSocketException or InvalidOperationException or OperationCanceledException)
        {
            throw new VeadotubeException($"Failed to connect to veadotube at {_options.Endpoint}.", ex);
        }
        _loopCts = new CancellationTokenSource();
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(_loopCts.Token), CancellationToken.None);
    }

    /// <summary>Closes the WebSocket transport cleanly.</summary>
    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_loopCts is null)
        {
            return;
        }

        try { await _loopCts.CancelAsync().ConfigureAwait(false); }
        catch (ObjectDisposedException) { }

        if (_ws is { State: WebSocketState.Open })
        {
            try { await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "client closing", ct).ConfigureAwait(false); }
            catch (WebSocketException) { }
            catch (OperationCanceledException) { }
        }

        if (_receiveLoop is not null)
        {
            try { await _receiveLoop.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
        }

        Cleanup();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try { await DisconnectAsync(CancellationToken.None).ConfigureAwait(false); }
        catch (VeadotubeException) { }
        _sendLock.Dispose();
    }

    // ----- Instance channel ---------------------------------------------------

    /// <summary>Requests the instance's <c>info</c> payload over the <see cref="VeadotubeApi.InstanceChannel"/> channel.</summary>
    public async Task<InstanceInfoResponse> GetInstanceInfoAsync(CancellationToken ct = default)
    {
        string key = $"{VeadotubeApi.InstanceChannel}.info";
        Task<JsonElement> waiter = RegisterWaiter(key);
        await SendInstanceAsync(new InstanceInfoRequest(), VeadotubeJsonContext.Default.InstanceInfoRequest, ct).ConfigureAwait(false);
        JsonElement raw = await AwaitWithTimeout(waiter, ct).ConfigureAwait(false);
        return raw.Deserialize(VeadotubeJsonContext.Default.InstanceInfoResponse) ?? throw new VeadotubeException("Empty instance info response.");
    }

    // ----- Nodes channel — listing -------------------------------------------

    /// <summary>Requests the current list of nodes exposed by the instance.</summary>
    public async Task<NodeListResponse> GetNodesAsync(CancellationToken ct = default)
    {
        string key = $"{VeadotubeApi.NodesChannel}.list";
        Task<JsonElement> waiter = RegisterWaiter(key);
        await SendNodesAsync(new NodeListRequest(), VeadotubeJsonContext.Default.NodeListRequest, ct).ConfigureAwait(false);
        JsonElement raw = await AwaitWithTimeout(waiter, ct).ConfigureAwait(false);
        return raw.Deserialize(VeadotubeJsonContext.Default.NodeListResponse) ?? throw new VeadotubeException("Empty node-list response.");
    }

    /// <summary>Subscribes to node-list change pushes; correlate with <see cref="UnlistenNodeListAsync"/> via <paramref name="token"/>.</summary>
    public Task ListenNodeListAsync(string? token = null, CancellationToken ct = default)
        => SendNodesAsync(new NodeListListenRequest { Token = token }, VeadotubeJsonContext.Default.NodeListListenRequest, ct);

    /// <summary>Cancels a node-list subscription issued via <see cref="ListenNodeListAsync"/>.</summary>
    public Task UnlistenNodeListAsync(string? token = null, CancellationToken ct = default)
        => SendNodesAsync(new NodeListUnlistenRequest { Token = token }, VeadotubeJsonContext.Default.NodeListUnlistenRequest, ct);

    // ----- Typed node accessors ----------------------------------------------

    /// <summary>Returns a typed accessor for a <c>boolean</c> node.</summary>
    public BooleanNodeAccessor Boolean(string nodeId) => new(this, nodeId);

    /// <summary>Returns a typed accessor for a <c>number</c> node.</summary>
    public NumberNodeAccessor Number(string nodeId) => new(this, nodeId);

    /// <summary>Returns a typed accessor for a <c>stateEvents</c> node.</summary>
    public StateEventsNodeAccessor States(string nodeId) => new(this, nodeId);

    // ----- Raw escape hatch --------------------------------------------------

    /// <summary>Sends a custom payload on the <see cref="VeadotubeApi.NodesChannel"/> channel — useful for forward-compat with future node types.</summary>
    public Task SendRawNodePayloadAsync(string nodeType, string nodeId, JsonElement payload, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeType);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        NodePayloadEnvelope envelope = new() { Type = nodeType, Id = nodeId, Payload = payload };
        return SendNodesAsync(envelope, VeadotubeJsonContext.Default.NodePayloadEnvelope, ct);
    }

    // ----- Internal: per-node send / await -----------------------------------

    internal async Task<TResponse> RequestNodeAsync<TRequest, TResponse>(
        string nodeType,
        string nodeId,
        TRequest payload,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TRequest> requestInfo,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TResponse> responseInfo,
        string expectedInnerEvent,
        CancellationToken ct)
        where TRequest : notnull
    {
        string key = $"{VeadotubeApi.NodesChannel}.{nodeType}.{nodeId}.{expectedInnerEvent}";
        Task<JsonElement> waiter = RegisterWaiter(key);
        await SendNodePayloadAsync(nodeType, nodeId, payload, requestInfo, ct).ConfigureAwait(false);
        JsonElement raw = await AwaitWithTimeout(waiter, ct).ConfigureAwait(false);
        return raw.Deserialize(responseInfo) ?? throw new VeadotubeException("Empty response payload.");
    }

    internal Task SendNodePayloadAsync<TRequest>(
        string nodeType,
        string nodeId,
        TRequest payload,
        System.Text.Json.Serialization.Metadata.JsonTypeInfo<TRequest> requestInfo,
        CancellationToken ct) where TRequest : notnull
    {
        JsonElement inner = JsonSerializer.SerializeToElement(payload, requestInfo);
        NodePayloadEnvelope envelope = new() { Type = nodeType, Id = nodeId, Payload = inner };
        return SendNodesAsync(envelope, VeadotubeJsonContext.Default.NodePayloadEnvelope, ct);
    }

    // ----- Send / receive ----------------------------------------------------

    private Task SendNodesAsync<T>(T payload, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> info, CancellationToken ct)
        where T : notnull
    {
        JsonElement inner = JsonSerializer.SerializeToElement(payload, info);
        return SendChannelAsync(VeadotubeApi.NodesChannel, inner, ct);
    }

    private Task SendInstanceAsync<T>(T payload, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> info, CancellationToken ct)
        where T : notnull
    {
        JsonElement inner = JsonSerializer.SerializeToElement(payload, info);
        return SendChannelAsync(VeadotubeApi.InstanceChannel, inner, ct);
    }

    private async Task SendChannelAsync(string channel, JsonElement payload, CancellationToken ct)
    {
        EnsureConnected();
        using MemoryStream buffer = new();
        await using (Utf8JsonWriter writer = new(buffer))
        {
            writer.WriteStartObject();
            writer.WritePropertyName(channel);
            payload.WriteTo(writer);
            writer.WriteEndObject();
        }

        ArraySegment<byte> bytes = new(buffer.GetBuffer(), 0, (int)buffer.Length);
        await _sendLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await _ws!.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct).ConfigureAwait(false);
        }
        finally { _ = _sendLock.Release(); }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        byte[] buffer = new byte[_options.ReceiveBufferSize];
        MemoryStream pending = new();
        try
        {
            while (!ct.IsCancellationRequested && _ws is { State: WebSocketState.Open })
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { break; }
                catch (WebSocketException) { break; }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.Count > 0)
                {
                    pending.Write(buffer, 0, result.Count);
                }

                if (!result.EndOfMessage)
                {
                    continue;
                }

                try
                {
                    JsonDocument doc = JsonDocument.Parse(pending.GetBuffer().AsMemory(0, (int)pending.Length));
                    DispatchMessage(doc.RootElement);
                    doc.Dispose();
                }
                catch (JsonException ex) { LogParseError(_logger, ex); }
                finally { pending.SetLength(0); }
            }
        }
        finally
        {
            try { Disconnected?.Invoke(this, EventArgs.Empty); } catch { /* swallow */ }
        }
    }

    private void DispatchMessage(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (root.TryGetProperty(VeadotubeApi.NodesChannel, out JsonElement nodesBody))
        {
            DispatchNodesMessage(nodesBody);
        }
        else if (root.TryGetProperty(VeadotubeApi.InstanceChannel, out JsonElement instanceBody))
        {
            DispatchInstanceMessage(instanceBody);
        }
    }

    private void DispatchInstanceMessage(JsonElement body)
    {
        if (!body.TryGetProperty("event", out JsonElement evtProp))
        {
            return;
        }

        string evt = evtProp.GetString() ?? string.Empty;
        Events.Dispatch($"instance.{evt}", body);
        Resolve($"{VeadotubeApi.InstanceChannel}.{evt}", body);
    }

    private void DispatchNodesMessage(JsonElement body)
    {
        if (!body.TryGetProperty("event", out JsonElement evtProp))
        {
            return;
        }

        string evt = evtProp.GetString() ?? string.Empty;

        if (string.Equals(evt, "payload", StringComparison.Ordinal))
        {
            if (!body.TryGetProperty("type", out JsonElement typeProp) || !body.TryGetProperty("id", out JsonElement idProp) || !body.TryGetProperty("payload", out JsonElement payloadElem))
            {
                return;
            }
            string nodeType = typeProp.GetString() ?? string.Empty;
            string nodeId = idProp.GetString() ?? string.Empty;
            string innerEvent = payloadElem.TryGetProperty("event", out JsonElement innerEvtProp) ? innerEvtProp.GetString() ?? string.Empty : string.Empty;

            Events.Dispatch($"{nodeType}.{innerEvent}", payloadElem);
            Resolve($"{VeadotubeApi.NodesChannel}.{nodeType}.{nodeId}.{innerEvent}", payloadElem);
            return;
        }

        // Channel-level events (e.g. node-list "list", "listen", "unlisten").
        Events.Dispatch($"{VeadotubeApi.NodesChannel}.{evt}", body);
        Resolve($"{VeadotubeApi.NodesChannel}.{evt}", body);
    }

    private Task<JsonElement> RegisterWaiter(string key)
    {
        TaskCompletionSource<JsonElement> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ConcurrentQueue<TaskCompletionSource<JsonElement>> queue = _pending.GetOrAdd(key, _ => new ConcurrentQueue<TaskCompletionSource<JsonElement>>());
        queue.Enqueue(tcs);
        return tcs.Task;
    }

    private void Resolve(string key, JsonElement payload)
    {
        if (_pending.TryGetValue(key, out ConcurrentQueue<TaskCompletionSource<JsonElement>>? queue) && queue.TryDequeue(out TaskCompletionSource<JsonElement>? tcs))
        {
            _ = tcs.TrySetResult(payload);
        }
    }

    private async Task<JsonElement> AwaitWithTimeout(Task<JsonElement> waiter, CancellationToken ct)
    {
        using CancellationTokenSource timeoutCts = new(_options.RequestTimeout);
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
        try
        {
            return await waiter.WaitAsync(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new VeadotubeException($"Timed out after {_options.RequestTimeout.TotalSeconds:0.#}s waiting for a veadotube response.");
        }
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
        {
            throw new VeadotubeException("The veadotube client is not connected.");
        }
    }

    private void Cleanup()
    {
        try { _ws?.Dispose(); } catch { /* swallow */ }
        _ws = null;
        try { _loopCts?.Dispose(); } catch { /* swallow */ }
        _loopCts = null;
        _receiveLoop = null;
        foreach (KeyValuePair<string, ConcurrentQueue<TaskCompletionSource<JsonElement>>> entry in _pending)
        {
            while (entry.Value.TryDequeue(out TaskCompletionSource<JsonElement>? tcs))
            {
                _ = tcs.TrySetException(new VeadotubeException("Connection closed before response arrived."));
            }
        }
        _pending.Clear();
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Failed to parse incoming veadotube message.")]
    private static partial void LogParseError(ILogger logger, Exception ex);
}
