using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Veadotube.Client.Events;

namespace Veadotube.Client.Tests;

[TestClass]
public sealed class EventHubTests
{
    [TestMethod]
    public void Dispatch_Invokes_Matching_Typed_Handler()
    {
        VeadotubeEventHub hub = new();
        string? received = null;
        using VeadotubeEventHub.Subscription _ = hub.On<StateChangedEventPayload>(p => received = p.State);

        JsonElement payload = JsonElement.Parse("""{"event":"peek","state":"awake"}""");
        hub.Dispatch("stateEvents.peek", payload);

        Assert.AreEqual("awake", received);
    }

    [TestMethod]
    public void Dispatch_Ignores_Unsubscribed_Events()
    {
        VeadotubeEventHub hub = new();
        bool fired = false;
        using VeadotubeEventHub.Subscription _ = hub.On<StateChangedEventPayload>(_ => fired = true);

        JsonElement payload = JsonElement.Parse("""{"event":"get","value":true}""");
        hub.Dispatch("boolean.get", payload);

        Assert.IsFalse(fired);
    }

    [TestMethod]
    public void Unsubscribe_Stops_Dispatch()
    {
        VeadotubeEventHub hub = new();
        int hits = 0;
        VeadotubeEventHub.Subscription sub = hub.On<StateChangedEventPayload>(_ => hits++);

        JsonElement payload = JsonElement.Parse("""{"event":"peek","state":"a"}""");
        hub.Dispatch("stateEvents.peek", payload);
        sub.Dispose();
        hub.Dispatch("stateEvents.peek", payload);

        Assert.AreEqual(1, hits);
    }
}
