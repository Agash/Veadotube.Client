# Veadotube.Client

Modern .NET 10 / C# 14 client library for the [veadotube](https://veado.tube) WebSocket API. AOT-friendly, `System.Text.Json` source-generated, fully typed, no `JObject` / `dynamic` anywhere.

Supports the documented [`nodes` channel](https://veado.tube/docs/tech/api/nodes/) and [`instance` channel](https://veado.tube/docs/tech/api/instance/) for veadotube mini and veadotube. Instance auto-discovery reads the `~/.veadotube/instances/` drop directory across Windows, Linux, and macOS.

## Install

```pwsh
dotnet add package Veadotube.Client
dotnet add package Veadotube.Client.DependencyInjection   # optional, IServiceCollection wiring
```

## Quick start — discover and drive a running veadotube mini

```csharp
using Veadotube.Client;
using Veadotube.Client.Discovery;

// 1. Discover a running instance.
VeadotubeInstanceInfo? instance = VeadotubeInstanceDiscovery.FindFirstAlive(typeFilter: "mini");
if (instance is null)
{
    throw new InvalidOperationException("No live veadotube mini instance found.");
}

// 2. Open the WebSocket.
await using VeadotubeClient client = new(new VeadotubeClientOptions
{
    Endpoint = instance.WebSocketEndpoint("StreamWeaver"),
});
await client.ConnectAsync();

// 3. List the nodes the instance exposes.
NodeListResponse list = await client.GetNodesAsync();
foreach (NodeListEntry entry in list.Entries)
{
    Console.WriteLine($"{entry.Type} : {entry.Id} — {entry.Name}");
}

// 4. Drive the avatar state.
StateListResponse states = await client.States("mini").ListAsync();
await client.States("mini").PushAsync(states.States[0].Id);
```

## Typed event subscriptions

```csharp
client.Events.On<StateChangedEventPayload>(p =>
    Console.WriteLine($"avatar is now: {p.State}"));

await client.States("mini").ListenAsync();
```

Each pushed event payload implements `IVeadotubeEvent<TSelf>` and carries its own `JsonTypeInfo<TSelf>` from the source-generated context, so the hub stays AOT-safe without per-call type info parameters.

## DI

```csharp
services.AddVeadotubeClient(options =>
{
    options.Endpoint = new Uri("ws://127.0.0.1:2424?n=MyApp");
});
```

## License

MIT. See [LICENSE.txt](LICENSE.txt).
