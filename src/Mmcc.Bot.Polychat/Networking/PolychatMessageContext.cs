using System;
using Google.Protobuf.WellKnownTypes;
using Ssmp;

namespace Mmcc.Bot.Polychat.Networking;

public class PolychatMessageContext
{
    public ConnectedClient? Author { get; set; }
    public Any? MessageContent { get; set; }
    
    public string? RawTypeUrl => MessageContent?.TypeUrl;

    public string? GetTelemetryMessageIdentifier()
    {
        if (RawTypeUrl is null) return null;
        
        var str = RawTypeUrl.AsSpan();
        var type = str[(str.IndexOf('/') + 1)..];
        var firstLetter = type[..1];
    
        type = type[1..];
    
        return firstLetter.ToString().ToUpperInvariant() + type.ToString();
    }
}