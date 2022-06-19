namespace Mmcc.Bot.Polychat.Networking;

/// <inheritdoc />>
public partial class RequestResolver : IRequestResolver
{
    private readonly PolychatMessageContext _msgContext;

    public RequestResolver(PolychatMessageContext msgContext)
        => _msgContext = msgContext;
}