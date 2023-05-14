using System.Diagnostics;
using System.Globalization;
using System.Text;
using static Mmcc.Bot.SourceGenerators.CommonContexts;
using static Mmcc.Bot.SourceGenerators.DiscordCommands.DiscordCommandGeneratorContexts;

namespace Mmcc.Bot.SourceGenerators.DiscordCommands;

/// <summary>
/// Generates a Discord command from a vertical slice architecture-style parent class.
/// </summary>
[Generator]
internal sealed class DiscordCommandGenerator : IIncrementalGenerator
{
    private static SymbolDisplayFormat TypeFormat
        => SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.ExpandNullable |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("GenerateDiscordCommandAttribute.g.cs", SourceText.From(DiscordCommandGeneratorAttributes.GenerateDiscordCommandAttribute, Encoding.UTF8))
        );

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(IsVsaClassCandidateSyntactically, SemanticTransform)
            .Where(static typesData => typesData.HasValue)
            .Select(static (typesData, _) => GetVsaClassContext(typesData!.Value))
            .Where(static context => context is not null);

        context.RegisterSourceOutput(provider, GenerateSource!);
    }

    private static VsaClassContext? GetVsaClassContext((INamedTypeSymbol VsaType, INamedTypeSymbol CmdGroupType, INamedTypeSymbol ViewType, AttributeData AttributeData) typesData)
    {
        var vsaNamespace = typesData.VsaType.ContainingNamespace.ToDisplayString();
        var vsaName = typesData.VsaType.Name;

        var requestInfo = GetRequestClassInfo(typesData.VsaType);
        if (requestInfo is null)
            return null;

        var discordCommandContext = GetDiscordCommandContext(typesData.CmdGroupType, typesData.ViewType,
            requestInfo.Value.Type, typesData.AttributeData.TargetArguments);
        var shouldHandleNullReturn = GetShouldHandleNullReturn(typesData.VsaType);

        return new VsaClassContext
        {
            Namespace = vsaNamespace,
            ClassName = vsaName,
            RequestClassContext = requestInfo.Value.Context,
            DiscordCommandContext = discordCommandContext,
            ShouldHandleNullReturn = shouldHandleNullReturn,
            RemoraConditionsAttributeContexts = GetRemoraConditionsAttributeContexts(typesData.AttributeData)
        };
    }

    private static IReadOnlyList<ConditionAttributeContext> GetRemoraConditionsAttributeContexts(AttributeData attributeData)
    {
        var conditionsAttributes = attributeData.RemoraConditionsAttributes;
        var context = conditionsAttributes
            .Select(attr => new ConditionAttributeContext
            {
                Namespace = attr.AttributeType.ContainingNamespace.ToDisplayString(),
                ClassName = attr.AttributeType.Name,
                ArgumentsValues = attr.Arguments?
                    .Select(arg => arg.Match(symbol => symbol.ToDisplayString(), expression => expression.ToFullString()))
                    .ToList()
            })
            .ToList();

        return context;
    }

    private static (INamedTypeSymbol Type, RequestClassContext Context)? GetRequestClassInfo(INamedTypeSymbol vsaType)
    {
        var typeMembers = vsaType.GetTypeMembers();
        INamedTypeSymbol? requestType = null;
        requestType ??= typeMembers.FirstOrDefault(x => x.Name.Equals("Query", StringComparison.Ordinal));
        requestType ??= typeMembers.FirstOrDefault(x => x.Name.Equals("Command", StringComparison.Ordinal));

        if (requestType is null)
            return null;

        var @namespace = requestType.ContainingNamespace.ToDisplayString();
        var className = requestType.Name;
        var properties = requestType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p is
            {
                Kind: SymbolKind.Property,
                DeclaredAccessibility: Accessibility.Public,
                IsStatic: false,
                SetMethod.IsInitOnly: true
            })
            .Select(p => new PropertyContext
            {
                Name = p.Name,
                Type = p.Type.ToDisplayString(TypeFormat)
            })
            .ToList();

        var context = new RequestClassContext
        {
            Namespace = @namespace,
            ClassName = className,
            Properties = properties
        };

        return (requestType, context);
    }

    private static DiscordCommandContext GetDiscordCommandContext(
        INamedTypeSymbol cmdGroupType,
        INamedTypeSymbol viewType,
        INamedTypeSymbol requestType,
        AttributeArgumentListSyntax attributeArgumentsSyntax
    )
    {
        var @namespace = cmdGroupType.ContainingNamespace.ToDisplayString();
        var className = cmdGroupType.Name;
        var args = attributeArgumentsSyntax.Arguments;
        var commandName = args[0].Expression.ToFullString();
        var commandDescription = args[1].Expression.ToFullString();
        var isGreedy = args[2].Expression.IsKind(SyntaxKind.TrueLiteralExpression);
        var aliases = args
            .Skip(3)
            .Select(x => x.Expression.ToFullString())
            .ToList();

        var matchedViewContext = GetViewContext(viewType, requestType);

        return new DiscordCommandContext
        {
            Namespace = @namespace,
            ClassName = className,
            CommandName = commandName,
            CommandDescription = commandDescription,
            IsGreedy = isGreedy,
            CommandAliases = aliases,
            MatchedView = matchedViewContext
        };
    }

    private static DiscordViewContext GetViewContext(INamedTypeSymbol viewType, INamedTypeSymbol requestType)
    {
        var @namespace = viewType.ContainingNamespace.ToDisplayString();
        var className = viewType.Name;
        var onEmptyMethod = viewType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m is
            {
                Kind: SymbolKind.Method,
                MethodKind: MethodKind.Ordinary,
                IsStatic: true,
                DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
                Name: "OnEmpty",
                Parameters:
                {
                    Length: 1
                } parameters
            } && SymbolEqualityComparer.Default.Equals(parameters[0].Type, requestType));

        return new DiscordViewContext
        {
            Namespace = @namespace,
            ClassName = className,
            HasOnEmpty = onEmptyMethod is not null
        };
    }

    private static bool GetShouldHandleNullReturn(INamedTypeSymbol vsaType)
    {
        var typeMembers = vsaType.GetTypeMembers();
        var handleMethod = typeMembers
            .FirstOrDefault(x => x.Name.Equals("Handler", StringComparison.Ordinal))?
            .GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m is
            {
                Kind: SymbolKind.Method,
                MethodKind: MethodKind.Ordinary,
                IsStatic: false,
                DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
                Name: "Handle"
            });
        if (handleMethod is null)
            return false;

        if (!handleMethod.ReturnType.OriginalDefinition.ToDisplayString()
                .Equals("System.Threading.Tasks.Task<TResult>", StringComparison.Ordinal))
        {
            return handleMethod.ReturnType.NullableAnnotation == NullableAnnotation.Annotated;
        }

        var returnTypeInsideTask = ((INamedTypeSymbol)handleMethod.ReturnType).TypeArguments.FirstOrDefault();
        if (returnTypeInsideTask is null)
            return false; // something weird would be going on;

        if (!returnTypeInsideTask.OriginalDefinition.ToDisplayString()
                .Equals("Remora.Results.Result<TEntity>", StringComparison.Ordinal))
        {
            return returnTypeInsideTask.NullableAnnotation == NullableAnnotation.Annotated;
        }

        var resultType = returnTypeInsideTask;
        var returnType = ((INamedTypeSymbol)resultType).TypeArguments.FirstOrDefault();
        return returnType?.NullableAnnotation is NullableAnnotation.Annotated;
    }

    private static bool IsVsaClassCandidateSyntactically(SyntaxNode node, CancellationToken ct)
        => node is ClassDeclarationSyntax
        {
            AttributeLists.Count: > 0,
            BaseList: null or { Types.Count: 0 }
        } candidate
           && candidate.Modifiers.Any(SyntaxKind.PartialKeyword)
           && !candidate.Modifiers.Any(SyntaxKind.StaticKeyword);

    private static (INamedTypeSymbol VsaType, INamedTypeSymbol CmdGroupType, INamedTypeSymbol ViewType, AttributeData AttributeData)? SemanticTransform(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        Debug.Assert(ctx.Node is ClassDeclarationSyntax);
        var candidate = Unsafe.As<ClassDeclarationSyntax>(ctx.Node);

        var symbol = ctx.SemanticModel.GetDeclaredSymbol(candidate, ct);
        var generateDiscordAttribute =
            ctx.SemanticModel.Compilation.GetTypeByMetadataName("Mmcc.Bot.SourceGenerators.DiscordCommands.GenerateDiscordCommandAttribute`1");

        if (symbol is not null
            && TryGetAttributeData(candidate, generateDiscordAttribute, ctx.SemanticModel, out var attributeData)
            && attributeData.HasValue)
        {
            var viewType = ctx.SemanticModel.Compilation.GetTypeByMetadataName($"{symbol.ContainingNamespace}.{symbol.Name}View");

            if (viewType is not null)
            {
                return (symbol, attributeData.Value.CmdGroupType, viewType, attributeData.Value);
            }
        }

        return null;
    }

    private static bool TryGetAttributeData(
        ClassDeclarationSyntax candidate,
        INamedTypeSymbol? target,
        SemanticModel semanticModel,
        out AttributeData? attributeData
    )
    {
        INamedTypeSymbol? cmdGroupType = null;
        AttributeArgumentListSyntax? targetArguments = null;
        var conditionAttributes = new List<(INamedTypeSymbol, IReadOnlyList<OneOf<IFieldSymbol, ExpressionSyntax>>?)>();
        foreach (var attributeList in candidate.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeSymbolInfo = semanticModel.GetSymbolInfo(attribute);
                var attributeSymbol = attributeSymbolInfo.Symbol;

                if (attributeSymbol is null)
                    continue;

                // Target attribute;
                if (attribute is
                    {
                        Name: GenericNameSyntax
                        {
                            TypeArgumentList.Arguments:
                            {
                                Count: 1
                            } typeArguments
                        },
                        ArgumentList:
                        {
                            Arguments.Count: >= 3
                        } attributeArgumentsSyntax
                    }
                    && SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingSymbol.OriginalDefinition, target)
                )
                {
                    var commandGroupSymbolCandidate = semanticModel.GetSymbolInfo(typeArguments[0]).Symbol;

                    if (commandGroupSymbolCandidate is INamedTypeSymbol commandGroupSymbol)
                    {
                        cmdGroupType = commandGroupSymbol;
                        targetArguments = attributeArgumentsSyntax;
                    }
                }
                // Remora conditions;
                else if (attribute.Name.ToString().StartsWith("Require"))
                {
                    var symbol = semanticModel.GetSymbolInfo(attribute).Symbol;
                    if (symbol is not IMethodSymbol methodSymbol)
                        continue;

                    var conditionAttributeType = methodSymbol.ContainingType;
                    var conditionAttributeNamespace = conditionAttributeType.ContainingNamespace.ToDisplayString();
                    if (!conditionAttributeNamespace.StartsWith("Remora.Discord.Commands.Conditions") && !conditionAttributeNamespace.StartsWith("Mmcc.Bot.RemoraAbstractions.Conditions"))
                        continue;

                    if (attribute.ArgumentList is null || attribute.ArgumentList.Arguments.Count == 0)
                    {
                        conditionAttributes.Add((conditionAttributeType, null));
                    }
                    else
                    {
                        var args = new List<OneOf<IFieldSymbol, ExpressionSyntax>>(attribute.ArgumentList.Arguments.Count);
                        foreach (var argSyntax in attribute.ArgumentList.Arguments)
                        {
                            var argSymbol = semanticModel.GetSymbolInfo(argSyntax.Expression).Symbol;
                            if (argSymbol is not IFieldSymbol { Type: INamedTypeSymbol argType } argFieldSymbol
                                || argType.EnumUnderlyingType is null
                            )
                            {
                                args.Add(argSyntax.Expression);
                            }
                            else
                            {
                                args.Add(OneOf<IFieldSymbol, ExpressionSyntax>.FromT0(argFieldSymbol));
                            }
                        }

                        conditionAttributes.Add((conditionAttributeType, args));
                    }
                }
            }
        }

        attributeData = cmdGroupType is null || targetArguments is null
            ? null
            : new AttributeData(cmdGroupType, targetArguments, conditionAttributes);

        return attributeData is not null;
    }
    
    private static void GenerateSource(SourceProductionContext productionContext, VsaClassContext vsaContext)
    {
        var sanitisedCommandName = vsaContext.DiscordCommandContext.CommandName.Replace("\"", "");
        var methodName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sanitisedCommandName);
        var props = vsaContext
            .RequestClassContext
            .Properties;

        var methodParamsString = GenerateDiscordCommandMethodParams(props, vsaContext.DiscordCommandContext.IsGreedy);
        var requestCtorParams = string.Join(", ", props.Select(p => p.Name.ToCamelCase()));

        var generatedSource = $$"""
        // auto-generated

        namespace {{vsaContext.DiscordCommandContext.Namespace}};
        
        public partial class {{vsaContext.DiscordCommandContext.ClassName}}
        {
        {{GenerateRemoraConditionAttributesString(vsaContext.RemoraConditionsAttributeContexts)}}
            [global::Remora.Commands.Attributes.Command({{vsaContext.DiscordCommandContext.CommandName}}{{GenerateAliasesString(vsaContext.DiscordCommandContext)}})]
            [global::System.ComponentModel.Description({{vsaContext.DiscordCommandContext.CommandDescription}})]
            public async global::System.Threading.Tasks.Task<global::Remora.Results.IResult> {{methodName}}({{methodParamsString}})
            {
                var request = new {{vsaContext.Namespace}}.{{vsaContext.ClassName}}.{{vsaContext.RequestClassContext.ClassName}}({{requestCtorParams}});
                var result = await _mediator.Send(request);
                
                return result switch
                {
                    { IsSuccess: true, Entity: {  } e }
                        => await _vm.RespondWithView(new {{vsaContext.DiscordCommandContext.MatchedView.Namespace}}.{{vsaContext.DiscordCommandContext.MatchedView.ClassName}}(e)),
                    {{GenerateNullHandlerIfNeeded(vsaContext)}}
                    { IsSuccess: false } => result
                };
            }
        }
        """;

        var fileName = $"{vsaContext.Namespace}.{vsaContext.ClassName}.dcmd.g.cs";
        productionContext.AddSource(fileName, generatedSource);
    }

    private static string GenerateAliasesString(DiscordCommandContext discordCommandContext)
    {
        var aliases = discordCommandContext.CommandAliases;
        var aliasesString = aliases.Any()
            ? $", {string.Join(", ", aliases)}"
            : string.Empty;

        return aliasesString;
    }

    private static string GenerateNullHandlerIfNeeded(VsaClassContext vsaClassContext)
    {
        if (!vsaClassContext.ShouldHandleNullReturn)
            return string.Empty;

        var viewContext = vsaClassContext.DiscordCommandContext.MatchedView;

        return !viewContext.HasOnEmpty
            ? """
            
                        { IsSuccess: true } => 
                            global::Remora.Results.Result.FromError(new global::Remora.Results.NotFoundError()),
        
            """
            : $$"""
            
                        { IsSuccess: true } => 
                            global::Remora.Results.Result.FromError(new global::Remora.Results.NotFoundError({{viewContext.Namespace}}.{{viewContext.ClassName}}.OnEmpty(request))),
        
            """;
    }

    private static string GenerateRemoraConditionAttributesString(IReadOnlyList<ConditionAttributeContext>? remoraConditionsAttributeContexts)
    {
        if (remoraConditionsAttributeContexts is null || remoraConditionsAttributeContexts.Count == 0)
            return string.Empty;

        const string indent = "    ";
        var sb = new StringBuilder();

        foreach (var attribute in remoraConditionsAttributeContexts)
        {
            var attributeString = attribute.ArgumentsValues is null || attribute.ArgumentsValues.Count == 0
                ? $"[{attribute.Namespace}.{attribute.ClassName}]"
                : $"[{attribute.Namespace}.{attribute.ClassName}({string.Join(", ", attribute.ArgumentsValues)})]";

            sb.AppendLine($"{indent}{attributeString}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string GenerateDiscordCommandMethodParams(IReadOnlyList<PropertyContext> props, bool isGreedy)
    {
        if (!isGreedy)
            return string.Join(", ", props.Select(p => $"{p.Type} {p.Name.ToCamelCase()}"));

        var sb = new StringBuilder();
        for (int i = 0; i < props.Count; i++)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            // justification: Trace - cleaner here imo;
            if (i == props.Count - 1)
            {
                sb.Append($"[global::Remora.Commands.Attributes.Greedy] {props[i].Type} {props[i].Name.ToCamelCase()}");
            }
            else
            {
                sb.Append($"{props[i].Type} {props[i].Name.ToCamelCase()}, ");
            }
        }

        return sb.ToString().TrimEnd();
    }
}

internal readonly struct AttributeData
{
    internal readonly INamedTypeSymbol CmdGroupType;
    internal readonly AttributeArgumentListSyntax TargetArguments;
    internal readonly IReadOnlyList<(INamedTypeSymbol AttributeType, IReadOnlyList<OneOf<IFieldSymbol, ExpressionSyntax>>? Arguments)> RemoraConditionsAttributes;

    internal AttributeData(
        INamedTypeSymbol cmdGroupType,
        AttributeArgumentListSyntax targetArguments,
        IReadOnlyList<(INamedTypeSymbol, IReadOnlyList<OneOf<IFieldSymbol, ExpressionSyntax>>?)> remoraConditionAttributes
    )
    {
        CmdGroupType = cmdGroupType;
        TargetArguments = targetArguments;
        RemoraConditionsAttributes = remoraConditionAttributes;
    }
}