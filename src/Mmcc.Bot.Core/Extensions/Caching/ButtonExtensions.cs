using Mmcc.Bot.Caching;
using Mmcc.Bot.Caching.Entities;

namespace Mmcc.Bot.Core.Extensions.Caching
{
    public static class ButtonExtensions
    {
        public static Button RegisterWith(this Button button, IButtonHandlerRepository repository)
        {
            repository.Register(button);
            return button;
        }
    }
}