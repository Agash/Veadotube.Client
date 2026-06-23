using System.Collections.Concurrent;
using System.Text.Json;

namespace Veadotube.Client.Events;

/// <summary>
/// Routes server-pushed events to subscribed handlers, keyed by the composite
/// <see cref="IVeadotubeEvent{TSelf}.EventName"/>. Handlers register typed callbacks; the
/// hub deserialises the matching <see cref="JsonElement"/> on dispatch.
/// </summary>
public sealed class VeadotubeEventHub
{
    private readonly ConcurrentDictionary<string, List<Action<JsonElement>>> _handlers = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();

    /// <summary>Register a typed handler. Returns an <see cref="IDisposable"/> token; dispose to unsubscribe.</summary>
    public Subscription On<TPayload>(Action<TPayload> handler)
        where TPayload : class, IVeadotubeEvent<TPayload>
    {
        ArgumentNullException.ThrowIfNull(handler);
        return OnCore(TPayload.EventName, json =>
        {
            TPayload? typed = json.Deserialize(TPayload.JsonTypeInfo);
            if (typed is not null)
            {
                handler(typed);
            }
        });
    }

    internal void Dispatch(string eventName, JsonElement payload)
    {
        if (!_handlers.TryGetValue(eventName, out List<Action<JsonElement>>? handlers))
        {
            return;
        }

        Action<JsonElement>[] snapshot;
        lock (_gate)
        {
            snapshot = [.. handlers];
        }

        foreach (Action<JsonElement> handler in snapshot)
        {
            try { handler(payload); }
            catch { /* swallow: handler exceptions must not break the receive loop. */ }
        }
    }

    private Subscription OnCore(string eventName, Action<JsonElement> handler)
    {
        List<Action<JsonElement>> handlers = _handlers.GetOrAdd(eventName, _ => []);
        lock (_gate) { handlers.Add(handler); }
        return new Subscription(() =>
        {
            lock (_gate) { _ = handlers.Remove(handler); }
        });
    }

    /// <summary>Disposable handle returned by <see cref="On{TPayload}"/>; dispose to unsubscribe.</summary>
    public sealed class Subscription : IDisposable
    {
        private Action? _dispose;
        internal Subscription(Action dispose)
        {
            _dispose = dispose;
        }
        /// <inheritdoc />
        public void Dispose()
        {
            Action? d = Interlocked.Exchange(ref _dispose, null);
            d?.Invoke();
        }
    }
}
