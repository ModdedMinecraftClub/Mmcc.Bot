using System;
using Microsoft.Extensions.Caching.Memory;
using Mmcc.Bot.Caching.Entities;

namespace Mmcc.Bot.Caching
{
    public interface IButtonHandlerRepository
    {
        void Register(Button button);
        void Register(ulong buttonId, ButtonHandler handler);
        void Deregister(ulong buttonId);
        ButtonHandler? GetOrDefault(ulong buttonId);
    }
    
    public class ButtonHandlerRepository : IButtonHandlerRepository
    {
        private readonly IMemoryCache _cache;

        public ButtonHandlerRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Register(Button button) =>
            Register(button.Id.Value, button.Handler);
        
        public void Register(ulong buttonId, ButtonHandler handler) =>
            _cache.Set(buttonId, handler, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

        public void Deregister(ulong buttonId) =>
            _cache.Remove(buttonId);
        
        public ButtonHandler? GetOrDefault(ulong buttonId) =>
            _cache.Get<ButtonHandler>(buttonId);
    }
}