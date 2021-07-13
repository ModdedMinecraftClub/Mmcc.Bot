using System;
using Microsoft.Extensions.Caching.Memory;
using Mmcc.Bot.Caching.Entities;

namespace Mmcc.Bot.Caching
{
    public interface IButtonHandlerRepository
    {
        void Register(Guid buttonGuid, ButtonHandler handler);
        void Deregister(Guid buttonGuid);
        ButtonHandler? GetOrDefault(Guid buttonGuid);
    }
    
    public class ButtonHandlerRepository : IButtonHandlerRepository
    {
        private readonly IMemoryCache _cache;

        public ButtonHandlerRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Register(Guid buttonGuid, ButtonHandler handler) =>
            _cache.Set(buttonGuid, handler, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

        public void Deregister(Guid buttonGuid) =>
            _cache.Remove(buttonGuid);
        
        public ButtonHandler? GetOrDefault(Guid buttonGuid) =>
            _cache.Get<ButtonHandler>(buttonGuid);
    }
}