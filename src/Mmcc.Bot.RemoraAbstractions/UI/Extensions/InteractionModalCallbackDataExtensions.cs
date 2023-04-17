using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.RemoraAbstractions.UI.Extensions;

public static class InteractionModalCallbackDataExtensions
{
    public static InteractionResponse GetInteractionResponse(this InteractionModalCallbackData modalData)
        => new(InteractionCallbackType.Modal, new(modalData));
}