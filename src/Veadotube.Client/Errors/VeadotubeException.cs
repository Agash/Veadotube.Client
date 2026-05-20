namespace Veadotube.Client.Errors;

/// <summary>Raised when the veadotube WebSocket connection or protocol surfaces an error.</summary>
public sealed class VeadotubeException : Exception
{
    /// <summary>Initialises a new instance with the supplied message.</summary>
    public VeadotubeException(string message) : base(message) { }

    /// <summary>Initialises a new instance with a message and inner exception.</summary>
    public VeadotubeException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>Initialises a new instance without arguments.</summary>
    public VeadotubeException() { }
}
