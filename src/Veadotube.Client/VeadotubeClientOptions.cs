namespace Veadotube.Client;

/// <summary>Configuration for a <see cref="VeadotubeClient"/>.</summary>
public sealed class VeadotubeClientOptions
{
    /// <summary>
    /// WebSocket endpoint. Typically of the form <c>ws://&lt;host&gt;:&lt;port&gt;?n=&lt;client-name&gt;</c>.
    /// Discover the host/port via <see cref="Discovery.VeadotubeInstanceDiscovery"/> or set explicitly.
    /// </summary>
    public Uri Endpoint { get; set; } = new("ws://127.0.0.1:2424?n=Veadotube.Client");

    /// <summary>How long to wait for the response to a single request before timing out.</summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Buffer size for the receive loop (bytes). Increase for very large messages (e.g. thumbnails).</summary>
    public int ReceiveBufferSize { get; set; } = 64 * 1024;
}
