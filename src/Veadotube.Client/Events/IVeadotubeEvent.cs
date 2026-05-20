using System.Text.Json.Serialization.Metadata;

namespace Veadotube.Client.Events;

/// <summary>
/// Static-abstract contract on every typed event payload pushed by veadotube. The hub
/// uses <see cref="EventName"/> to dispatch and <see cref="JsonTypeInfo"/> to deserialise
/// without per-call type metadata arguments, keeping the call site AOT-safe.
/// </summary>
/// <remarks>
/// <see cref="EventName"/> is composed as <c>"{nodeType}.{innerEvent}"</c>, e.g.
/// <c>"stateEvents.peek"</c>. The hub computes the same key from received envelopes.
/// </remarks>
public interface IVeadotubeEvent<TSelf>
    where TSelf : class, IVeadotubeEvent<TSelf>
{
    /// <summary>Composite event identifier <c>"{nodeType}.{innerEvent}"</c>.</summary>
    static abstract string EventName { get; }

    /// <summary>Source-generated JSON type info used to deserialise the inner payload.</summary>
    static abstract JsonTypeInfo<TSelf> JsonTypeInfo { get; }
}
