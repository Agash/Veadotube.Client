using Spectre.Console;
using Veadotube.Client;
using Veadotube.Client.Discovery;
using Veadotube.Client.Events;
using Veadotube.Client.Messages;

AnsiConsole.Write(new FigletText("veadotube").Color(Color.HotPink));
AnsiConsole.MarkupLine("[grey]Interactive sample for Veadotube.Client[/]");

// 1. Discovery
IReadOnlyList<VeadotubeInstanceInfo> instances = VeadotubeInstanceDiscovery.Enumerate();
if (instances.Count == 0)
{
    AnsiConsole.MarkupLine("[red]No live veadotube instances found.[/] Open veadotube mini and try again.");
    return;
}

VeadotubeInstanceInfo instance = instances.Count == 1
    ? instances[0]
    : AnsiConsole.Prompt(
        new SelectionPrompt<VeadotubeInstanceInfo>()
            .Title("Pick a veadotube instance:")
            .UseConverter(i => $"{i.Name} [{i.Id}] @ {i.Server}")
            .AddChoices(instances));

AnsiConsole.MarkupLine($"[green]Connecting[/] to {instance.WebSocketEndpoint("Veadotube.Client.Sample")}");

await using VeadotubeClient client = new(new VeadotubeClientOptions
{
    Endpoint = instance.WebSocketEndpoint("Veadotube.Client.Sample"),
});

client.Disconnected += (_, _) => AnsiConsole.MarkupLine("[yellow]disconnected[/]");
client.Events.On<StateChangedEventPayload>(p => AnsiConsole.MarkupLine($"[cyan]state changed:[/] {p.State ?? "(empty)"}"));
client.Events.On<BooleanChangedEventPayload>(p => AnsiConsole.MarkupLine($"[cyan]boolean changed:[/] {p.Value?.ToString() ?? "(unset)"}"));
client.Events.On<NumberChangedEventPayload>(p => AnsiConsole.MarkupLine($"[cyan]number changed:[/] {p.Value}"));
client.Events.On<NodeListChangedEventPayload>(p => AnsiConsole.MarkupLine($"[cyan]node list changed:[/] {p.Entries.Count} entries"));

await client.ConnectAsync();
AnsiConsole.MarkupLine("[green]connected[/]");

InstanceInfoResponse info = await client.GetInstanceInfoAsync();
AnsiConsole.MarkupLine($"  name: {info.Name}, version: {info.Version}, language: {info.Language}");

NodeListResponse nodes = await client.GetNodesAsync();
Table nodeTable = new Table().AddColumns("Type", "Id", "Name");
foreach (NodeListEntry entry in nodes.Entries) nodeTable.AddRow(entry.Type, entry.Id, entry.Name);
AnsiConsole.Write(nodeTable);

while (true)
{
    string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Action?")
            .AddChoices("List states", "Peek state", "Push state", "Pop", "Toggle state", "Clear stack", "Toggle push-to-talk", "Listen to a node", "Quit"));

    try
    {
        switch (choice)
        {
            case "List states":
            {
                NodeListEntry node = PickStateNode(nodes);
                StateListResponse list = await client.States(node.Id).ListAsync();
                foreach (StateInfo s in list.States) AnsiConsole.MarkupLine($"  [yellow]{s.Id}[/] — {s.Name}");
                break;
            }
            case "Peek state":
            {
                NodeListEntry node = PickStateNode(nodes);
                StatePeekResponse peek = await client.States(node.Id).PeekAsync();
                AnsiConsole.MarkupLine($"  current: [yellow]{peek.State ?? "(empty)"}[/]");
                break;
            }
            case "Push state":
            {
                NodeListEntry node = PickStateNode(nodes);
                string id = AnsiConsole.Ask<string>("state id?");
                await client.States(node.Id).PushAsync(id);
                AnsiConsole.MarkupLine("[green]sent[/]");
                break;
            }
            case "Pop":
            {
                NodeListEntry node = PickStateNode(nodes);
                await client.States(node.Id).PopAsync();
                AnsiConsole.MarkupLine("[green]sent[/]");
                break;
            }
            case "Toggle state":
            {
                NodeListEntry node = PickStateNode(nodes);
                string id = AnsiConsole.Ask<string>("state id?");
                await client.States(node.Id).ToggleAsync(id);
                AnsiConsole.MarkupLine("[green]sent[/]");
                break;
            }
            case "Clear stack":
            {
                NodeListEntry node = PickStateNode(nodes);
                await client.States(node.Id).ClearAsync();
                AnsiConsole.MarkupLine("[green]sent[/]");
                break;
            }
            case "Toggle push-to-talk":
            {
                NodeListEntry node = PickBooleanNode(nodes);
                await client.Boolean(node.Id).ToggleAsync();
                AnsiConsole.MarkupLine("[green]sent[/]");
                break;
            }
            case "Listen to a node":
            {
                NodeListEntry node = AnsiConsole.Prompt(new SelectionPrompt<NodeListEntry>().Title("which node?").UseConverter(n => $"{n.Type} {n.Id}").AddChoices(nodes.Entries));
                Task listenTask = node.Type switch
                {
                    "stateEvents" => client.States(node.Id).ListenAsync("sample"),
                    "boolean"     => client.Boolean(node.Id).ListenAsync("sample"),
                    "number"      => client.Number(node.Id).ListenAsync("sample"),
                    _             => Task.CompletedTask,
                };
                await listenTask;
                AnsiConsole.MarkupLine("[green]subscribed[/]");
                break;
            }
            case "Quit":
                return;
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]error:[/] {ex.Message}");
    }
}

static NodeListEntry PickStateNode(NodeListResponse nodes)
    => AnsiConsole.Prompt(new SelectionPrompt<NodeListEntry>().Title("which stateEvents node?").UseConverter(n => $"{n.Type} {n.Id}").AddChoices(nodes.Entries.Where(e => e.Type == "stateEvents")));

static NodeListEntry PickBooleanNode(NodeListResponse nodes)
    => AnsiConsole.Prompt(new SelectionPrompt<NodeListEntry>().Title("which boolean node?").UseConverter(n => $"{n.Type} {n.Id}").AddChoices(nodes.Entries.Where(e => e.Type == "boolean")));
