namespace Mmcc.Bot.Polychat.Networking;

/// <summary>
/// Service for resolving a matching MediatR request for a Polychat message. 
/// </summary>
public interface IRequestResolver
{
    /// <summary>
    /// Resolves a matching <see cref="IPolychatRequest"/> for a Polychat message. 
    /// </summary>
    ///
    /// <returns>Matched <see cref="IPolychatRequest"/>. Returns <code>null</code> if no match could be found.</returns>
    IPolychatRequest? Resolve();
}