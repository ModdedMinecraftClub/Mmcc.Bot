using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using MediatR;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Objects;
using Remora.Results;

namespace Mmcc.Bot.Commands.Core.Help;

/// <summary>
/// Gets help for all available commands within a category.
/// </summary>
public class GetHelpForCategory
{
    /// <summary>
    /// Query to get help embeds for all available commands within a category.
    /// </summary>
    /// <param name="CategoryName">The name/alias of the category.</param>
    public record Query(string CategoryName) : IRequest<Result<Embed?>>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator() =>
            RuleFor(q => q.CategoryName)
                .NotEmpty();
    }
        
    public class Handler : RequestHandler<Query, Result<Embed?>>
    {
        private readonly CommandTree _commandTree;
        private readonly IHelpService _helpService;

        public Handler(CommandTree commandTree, IHelpService helpService)
        {
            _commandTree = commandTree;
            _helpService = helpService;
        }

        protected override Result<Embed?> Handle(Query request)
        {
            var categoryName = request.CategoryName;
            var categories = _commandTree.Root.Children.ToList()
                .OfType<GroupNode>()
                .FirstOrDefault(gn => gn.Key.Equals(categoryName) || gn.Aliases.Contains(categoryName));
            var embeds = new List<Embed>();

            if (categories is null)
            {
                return Result<Embed?>.FromSuccess(null);
            }

            _helpService.TraverseAndGetHelpEmbeds(categories.Children.ToList(), embeds);

            return Result<Embed?>.FromSuccess(embeds.FirstOrDefault());
        }
    }
}