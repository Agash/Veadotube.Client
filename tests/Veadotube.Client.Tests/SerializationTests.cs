using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Veadotube.Client.Discovery;
using Veadotube.Client.Messages;
using Veadotube.Client.Serialization;

namespace Veadotube.Client.Tests;

[TestClass]
public sealed class SerializationTests
{
    [TestMethod]
    public void InstanceInfo_Roundtrips()
    {
        const string json = """{"time":1739023315,"name":"veadotube mini","id":"mini-08dd3cd72f015bb9-000681a2","version":"2.1","language":"en","server":"127.0.0.1:2424"}""";
        VeadotubeInstanceInfo? info = JsonSerializer.Deserialize(json, VeadotubeJsonContext.Default.VeadotubeInstanceInfo);
        Assert.IsNotNull(info);
        Assert.AreEqual("veadotube mini", info!.Name);
        Assert.AreEqual("127.0.0.1:2424", info.Server);
        Assert.AreEqual("mini", info.InstanceType);
    }

    [TestMethod]
    public void NodeListResponse_Deserialises_With_Entries()
    {
        const string json = """{"event":"list","entries":[{"type":"boolean","id":"mini","name":"push-to-talk"},{"type":"stateEvents","id":"mini","name":"avatar state"}]}""";
        NodeListResponse? list = JsonSerializer.Deserialize(json, VeadotubeJsonContext.Default.NodeListResponse);
        Assert.IsNotNull(list);
        Assert.AreEqual(2, list!.Entries.Count);
        Assert.AreEqual("stateEvents", list.Entries[1].Type);
    }

    [TestMethod]
    public void StatePushRequest_Has_State_Field()
    {
        StatePushRequest req = new() { State = "awake" };
        string json = JsonSerializer.Serialize(req, VeadotubeJsonContext.Default.StatePushRequest);
        StringAssert.Contains(json, "\"event\":\"push\"");
        StringAssert.Contains(json, "\"state\":\"awake\"");
    }

    [TestMethod]
    public void WebSocketEndpoint_Encodes_ClientName()
    {
        VeadotubeInstanceInfo info = new() { Server = "127.0.0.1:2424" };
        Uri uri = info.WebSocketEndpoint("My App");
        Assert.AreEqual("ws://127.0.0.1:2424/?n=My%20App", uri.AbsoluteUri);
    }
}
