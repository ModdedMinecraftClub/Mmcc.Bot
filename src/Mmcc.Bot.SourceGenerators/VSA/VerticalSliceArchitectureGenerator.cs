using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Mmcc.Bot.SourceGenerators.VSA;

[Generator]
internal sealed class VerticalSliceArchitectureGenerator : IIncrementalGenerator
{
    private static SymbolDisplayFormat TypeFormat
        => SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.ExpandNullable |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => 
            ctx.AddSource("MapDiscordCommandAttribute.g.cs", SourceText.From(Attributes.GenerateDiscordFromMediatRAttribute, Encoding.UTF8))
        );        

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(IsVsaClassCandidateSyntactically, SemanticTransform)
            .Where(static typesData => typesData.HasValue)
            .Select(static (typesData, ct) => GetVSAClassContext(typesData!.Value, ct))
            .Where(static context => context is not null);
        
        context.RegisterSourceOutput(provider, GenerateSource!);
    }

    private static void GenerateSource(SourceProductionContext productionContext, Contexts.VSAClassContext vsaContext)
    {
        var sanitisedCommandName = vsaContext.DiscordCommandContext.CommandName.Replace("\"", "");
        var methodName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sanitisedCommandName);
        var props = vsaContext
            .RequestClassContext
            .Properties;

        var methodParamsString = BuildDiscordCommandMethodParams(props, vsaContext.DiscordCommandContext.IsGreedy);
        var requestCtorParams = string.Join(", ", props.Select(p => p.Name.ToCamelCase()));
        
        var generatedSource = $$"""
        // auto-generated

        namespace {{vsaContext.DiscordCommandContext.Namespace}};
        
        public partial class {{vsaContext.DiscordCommandContext.ClassName}}
        {
        {{GenerateRemoraConditionArgumentsString(vsaContext.RemoraConditionsArguments)}}
            [global::Remora.Commands.Attributes.Command({{vsaContext.DiscordCommandContext.CommandName}})]
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

    private static string GenerateNullHandlerIfNeeded(Contexts.VSAClassContext vsaClassContext)
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

    private static string GenerateRemoraConditionArgumentsString(IEnumerable<string> remoraConditionArguments)
    {
        const string indent = "    ";
        var sb = new StringBuilder();

        foreach (var argument in remoraConditionArguments)
        {
            sb.AppendLine($"{indent}{argument}");
        }

        return sb.ToString().TrimEnd();
    }

    private static string BuildDiscordCommandMethodParams(IReadOnlyList<Contexts.PropertyContext> props, bool isGreedy)
    {
        if (!isGreedy)
            return string.Join(", ", props.Select(p => $"{p.Type} {p.Name.ToCamelCase()}"));

        var sb = new StringBuilder();
        for (int i = 0; i < props.Count; i++)
        {
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

    private static Contexts.VSAClassContext? GetVSAClassContext((INamedTypeSymbol VsaType, INamedTypeSymbol CmdGroupType, INamedTypeSymbol ViewType, AttributeArgumentListSyntax AttributeArguments, List<string> RemoraConditionsArgs) typesData, CancellationToken ct)
    {        
        var vsaNamespace = typesData.VsaType.ContainingNamespace.ToDisplayString();
        var vsaName = typesData.VsaType.Name;
        
        var requestInfo = GetRequestClassInfo(typesData.VsaType);
        if (requestInfo is null)
            return null;

        var discordCommandContext = GetDiscordCommandContext(typesData.CmdGroupType, typesData.ViewType,
            requestInfo.Value.Type, typesData.AttributeArguments);
        var shouldHandleNullReturn = GetShouldHandleNullReturn(typesData.VsaType);

        return new Contexts.VSAClassContext
        {
            Namespace = vsaNamespace,
            ClassName = vsaName,
            RequestClassContext = requestInfo.Value.Context,
            DiscordCommandContext = discordCommandContext,
            ShouldHandleNullReturn = shouldHandleNullReturn,
            RemoraConditionsArguments = typesData.RemoraConditionsArgs
        };
    }

    private static (INamedTypeSymbol Type, Contexts.RequestClassContext Context)? GetRequestClassInfo(INamedTypeSymbol vsaType)
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
            .Select(p => new Contexts.PropertyContext
            {
                Name = p.Name,
                Type = p.Type.ToDisplayString(TypeFormat)
            })
            .ToList();

        var context = new Contexts.RequestClassContext
        {
            Namespace = @namespace,
            ClassName = className,
            Properties = properties
        };

        return (requestType, context);
    }

    private static Contexts.DiscordCommandContext GetDiscordCommandContext(
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

        return new Contexts.DiscordCommandContext
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
    
    private static Contexts.DiscordViewContext GetViewContext(INamedTypeSymbol viewType, INamedTypeSymbol requestType)
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

        return new Contexts.DiscordViewContext
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

    private static (INamedTypeSymbol VsaType, INamedTypeSymbol CmdGroupType, INamedTypeSymbol ViewType, AttributeArgumentListSyntax AttributeArguments, List<string> RemoraConditionsArgs)? SemanticTransform(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        Debug.Assert(ctx.Node is ClassDeclarationSyntax);
        var candidate = Unsafe.As<ClassDeclarationSyntax>(ctx.Node);

        var symbol = ctx.SemanticModel.GetDeclaredSymbol(candidate, ct);
        var generateDiscordAttribute =
            ctx.SemanticModel.Compilation.GetTypeByMetadataName("Mmcc.Bot.SourceGenerators.VSA.MapDiscordCommandAttribute`1");

        if (symbol is not null 
            && TryGetAttributeData(candidate, generateDiscordAttribute, ctx.SemanticModel, out var attributeData)
            && attributeData.HasValue)
        {
            var viewType = ctx.SemanticModel.Compilation.GetTypeByMetadataName($"{symbol.ContainingNamespace}.{symbol.Name}View");

            if (viewType is not null)
            {
                return (symbol, attributeData.Value.CmdGroupType, viewType, attributeData.Value.Arguments, attributeData.Value.RemoraConditionsArgs);
            }
        }

        return null;
    }

    private static bool TryGetAttributeData(
        ClassDeclarationSyntax candidate,
        INamedTypeSymbol? target,
        SemanticModel semanticModel,
        out (
            INamedTypeSymbol CmdGroupType,
            AttributeArgumentListSyntax Arguments,
            List<string> RemoraConditionsArgs
        )? attributeData
    )
    {
        INamedTypeSymbol? cmdGroupType = null;
        AttributeArgumentListSyntax? targetArguments = null;
        var conditionAttributes = new List<string>();        
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
                        conditionAttributes.Add($"[{conditionAttributeType.ToDisplayString()}]");
                    }
                    else
                    {                        
                        var args = new List<string>(attribute.ArgumentList.Arguments.Count);
                        foreach (var argSyntax in attribute.ArgumentList.Arguments)
                        {
                            var argSymbol = semanticModel.GetSymbolInfo(argSyntax.Expression).Symbol;
                            if (argSymbol is not IFieldSymbol argFieldSymbol)
                                continue;
                            if (argFieldSymbol.Type is not INamedTypeSymbol argType)
                                continue;

                            var argString = argType.EnumUnderlyingType is not null
                                ? argSymbol.ToDisplayString()
                                : argSyntax.Expression.ToFullString();

                            args.Add(argString);
                        }

                        var fullAttributeString = $"[{conditionAttributeType.ToDisplayString()}({string.Join(", ", args)})]";
                        conditionAttributes.Add(fullAttributeString);
                    }                    
                }
            }
        }

        attributeData = cmdGroupType is null || targetArguments is null
            ? null
            : (cmdGroupType, targetArguments, conditionAttributes);

        return attributeData is not null;
    }
}
