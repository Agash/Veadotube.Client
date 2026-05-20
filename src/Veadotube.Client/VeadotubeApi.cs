namespace Veadotube.Client;

/// <summary>Wire-format constants for the veadotube WebSocket API.</summary>
public static class VeadotubeApi
{
    /// <summary>Documented WebSocket channel for instance metadata (<c>info</c>).</summary>
    public const string InstanceChannel = "instance";

    /// <summary>Documented WebSocket channel for node discovery and node payload routing.</summary>
    public const string NodesChannel = "nodes";

    /// <summary>The built-in node type for binary on/off values (e.g. veadotube mini's push-to-talk).</summary>
    public const string BooleanNodeType = "boolean";

    /// <summary>The built-in node type for scalar numeric values, optionally with a range.</summary>
    public const string NumberNodeType = "number";

    /// <summary>The built-in node type for avatar state stacks (e.g. veadotube mini's <c>mini</c> node).</summary>
    public const string StateEventsNodeType = "stateEvents";

    /// <summary>Recommended query-string parameter for tagging your client in the WebSocket URL.</summary>
    public const string NameQueryParameter = "n";
}
