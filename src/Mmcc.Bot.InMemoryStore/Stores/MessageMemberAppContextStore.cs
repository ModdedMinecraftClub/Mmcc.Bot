using Microsoft.Extensions.Caching.Memory;

namespace Mmcc.Bot.InMemoryStore.Stores;

public interface IMessageMemberAppContextStore
{
    void Add(ulong messageId, int memberAppId);
    void Remove(ulong messageId);
    int? GetOrDefault(ulong key);
}

public class MessageMemberAppContextStore : IMessageMemberAppContextStore
{
    private readonly IMemoryCache _memCache;
    
    private readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(15);
    public readonly TimeSpan _absoluteExpiration = TimeSpan.FromHours(1);

    public MessageMemberAppContextStore(IMemoryCache memCache) 
        => _memCache = memCache;

    public void Add(ulong messageId, int memberAppId)
        => _memCache.Set(messageId, memberAppId, new MemoryCacheEntryOptions
        {
            SlidingExpiration = _slidingExpiration,
            AbsoluteExpirationRelativeToNow = _absoluteExpiration
        });

    public void Remove(ulong messageId)
        => _memCache.Remove(messageId);

    public int? GetOrDefault(ulong key) 
        => _memCache.Get<int?>(key);
}