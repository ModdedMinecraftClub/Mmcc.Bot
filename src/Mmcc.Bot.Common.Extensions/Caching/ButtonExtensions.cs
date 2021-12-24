using Mmcc.Bot.Caching;
using Mmcc.Bot.Caching.Entities;

namespace Mmcc.Bot.Common.Extensions.Caching
{
    public static class ButtonExtensions
    {
        public static HandleableButton RegisterWith(this HandleableButton handleableButton, IButtonHandlerRepository repository)
        {
            repository.Register(handleableButton);
            return handleableButton;
        }
    }
}