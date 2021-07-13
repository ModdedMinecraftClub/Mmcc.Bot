using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mmcc.Bot.Caching.Entities;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions
{
    public class ButtonBuilder
    {
        private Guid _guid;
        private ButtonComponent _buttonComponent;
        private ButtonHandler? _buttonHandler; 

        public ButtonBuilder(ButtonComponentStyle style)
        {
            _guid = new Guid();
            _buttonComponent = new(style);
            _buttonHandler = default;
        }

        public ButtonBuilder WithLabel(string label)
        {
            _buttonComponent = _buttonComponent with { Label = label };
            return this;
        }

        public ButtonBuilder WithEmoji(string unicode)
        {
            _buttonComponent = _buttonComponent with { Emoji = new PartialEmoji(Name: unicode) };
            return this;
        }

        public ButtonBuilder WithEmoji(ulong id)
        {
            _buttonComponent = _buttonComponent with { Emoji = new PartialEmoji(new Snowflake(id)) };
            return this;
        }

        public ButtonBuilder WithEmoji(Snowflake id)
        {
            _buttonComponent = _buttonComponent with { Emoji = new PartialEmoji(id) };
            return this;
        }

        public ButtonBuilder WithUrl(string url)
        {
            if (_buttonComponent.Style is not ButtonComponentStyle.Link)
            {
                throw new InvalidOperationException("ButtonComponentStyle must be set to Link to use WithUrl.");
            }
            
            _buttonComponent = _buttonComponent with { URL = url };
            return this;
        }

        public ButtonBuilder AsDisabled()
        {
            _buttonComponent = _buttonComponent with { IsDisabled = true };
            return this;
        }

        public ButtonBuilder WithHandler(
            Func<IInteractionCreate, Task<Result>> handle,
            Optional<DiscordPermission> requiredPermission = new()
        )
        {
            _buttonHandler = new ButtonHandler(handle, requiredPermission);
            return this;
        }

        public Button Build()
        {
            if (_buttonHandler is null)
            {
                throw new Exception("No handler");
            }

            var builtComponent = _buttonComponent with { CustomID = _guid.ToString() };
            return new(_guid, builtComponent, _buttonHandler);
        }
    }
}