using System.ComponentModel;
using MediatR;
using Porbeagle;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;

namespace Mmcc.Bot.Features.Diagnostics;

[Group("diagnostics")]
[Description("Server and bot diagnostics")]
public sealed partial class DiagnosticsCommands : CommandGroup
{
    private readonly IMediator _mediator;
    private readonly IContextAwareViewManager _vm;

    public DiagnosticsCommands(IMediator mediator, IContextAwareViewManager vm)
    {
        _mediator = mediator;
        _vm = vm;
    }
}