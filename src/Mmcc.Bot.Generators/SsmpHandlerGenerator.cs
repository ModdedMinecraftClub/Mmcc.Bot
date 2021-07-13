using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mmcc.Bot.Generators
{
    [Generator]
    public class SsmpHandlerGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (SyntaxReceiver)context.SyntaxContextReceiver!;
            var classes = receiver.Classes;
            var templateGenerator = new TemplateGenerator(receiver.SsmpInterfaceSymbol!,
                receiver.MediatRInterfaceSymbol!, receiver.ProtobufAnySymbol!, receiver.LoggerSymbol!, receiver.ScopeFactorySymbol!, classes);
            var generatedCode = templateGenerator.Generate();

            context.AddSource("SsmpHandler.cs", generatedCode);
        }

        public void Initialize(GeneratorInitializationContext context) =>
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            private INamedTypeSymbol? _messageInterfaceSymbol;

            public SyntaxReceiver() =>
                Classes = new();

            public INamedTypeSymbol? SsmpInterfaceSymbol { get; private set; }
            public INamedTypeSymbol? MediatRInterfaceSymbol { get; private set; }
            public INamedTypeSymbol? ProtobufAnySymbol { get; private set; }
            public INamedTypeSymbol? LoggerSymbol { get; private set; }
            public INamedTypeSymbol? ScopeFactorySymbol { get; private set; }
            public List<ClassDeclarationSyntax> Classes { get; }

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                _messageInterfaceSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("Google.Protobuf.IBufferMessage");
                
                SsmpInterfaceSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("Ssmp.ISsmpHandler");
                MediatRInterfaceSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("MediatR.IMediator");
                ProtobufAnySymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("Google.Protobuf.WellKnownTypes.Any");
                LoggerSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
                ScopeFactorySymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceScopeFactory");

                if (context.Node is ClassDeclarationSyntax { BaseList: { } baseList } cds
                    && baseList.Types.Any(t => IsModuleClass(t, context.SemanticModel, _messageInterfaceSymbol!)))
                {
                    Classes.Add(cds);
                }
            }

            private static bool IsModuleClass(BaseTypeSyntax baseType,
                SemanticModel model, INamedTypeSymbol moduleType)
            {
                var typeInfo = model.GetTypeInfo(baseType.Type);

                return SymbolEqualityComparer.Default.Equals(typeInfo.Type, moduleType);
            }
        }

        private class TemplateGenerator : TemplateGeneratorBase
        {
            private const string ValueTask = "global::System.Threading.Tasks.ValueTask";
            private const string GeneratedNamespace = "Mmcc.Bot.Polychat";

            private readonly INamedTypeSymbol _ssmpInterfaceSymbol;
            private readonly INamedTypeSymbol _mediatRInterfaceSymbol;
            private readonly INamedTypeSymbol _protobufAnySymbol;
            private readonly INamedTypeSymbol _loggerSymbol;
            private readonly INamedTypeSymbol _scopeFactorySymbol;

            private readonly List<ClassDeclarationSyntax> _classes;

            private readonly string _generatedType = $"{GeneratedNamespace}.SsmpHandler";

            public TemplateGenerator(
                INamedTypeSymbol ssmpInterfaceSymbol,
                INamedTypeSymbol mediatRInterfaceSymbol,
                INamedTypeSymbol protobufAnySymbol,
                INamedTypeSymbol loggerSymbol,
                INamedTypeSymbol scopeFactorySymbol,
                List<ClassDeclarationSyntax> classes
            )
            {
                _ssmpInterfaceSymbol = ssmpInterfaceSymbol;
                _mediatRInterfaceSymbol = mediatRInterfaceSymbol;
                _protobufAnySymbol = protobufAnySymbol;
                _loggerSymbol = loggerSymbol;
                _scopeFactorySymbol = scopeFactorySymbol;
                _classes = classes;
            }

            private string AnnotatedLogger =>
                $"{AnnotateTypeWithGlobal(_loggerSymbol)}<{AnnotateTypeWithGlobal(_generatedType)}>";
            
            private string AnnotateMsgType(string type) => $"global::{GeneratedNamespace}.{type}";

            protected override string FillInStub(string generatedFiller) =>
                $@"
using global::Microsoft.Extensions.Logging;
using global::Microsoft.Extensions.DependencyInjection;

// auto-generated
namespace {GeneratedNamespace}
{{
    public class SsmpHandler : {AnnotateTypeWithGlobal(_ssmpInterfaceSymbol)}
    {{
        private readonly {AnnotateTypeWithGlobal(_scopeFactorySymbol)} _scopeFactory;
        private readonly {AnnotatedLogger} _logger;
        
        public SsmpHandler({AnnotateTypeWithGlobal(_scopeFactorySymbol)} scopeFactory, {AnnotatedLogger} logger)
        {{
            _scopeFactory = scopeFactory;
            _logger = logger;
        }}
        
        public async {ValueTask} Handle(global::Ssmp.ConnectedClient connectedClient, byte[] message)
        {{
{string.Join("\n", generatedFiller.Split('\n').Select(s => Indent(s, 3)))}
        }}
    }}
}}
";

            protected override string GenerateFiller()
            {
                if (!_classes.Any())
                {
                    var stubSb = new StringBuilder("// generated code will go here;");

                    stubSb.AppendLine($"return {ValueTask}.CompletedTask;");

                    return stubSb.ToString();
                }

                var sb = new StringBuilder();
                var messages = _classes.Select(c => AnnotateMsgType(c.Identifier.ValueText)).ToList();
                var firstMessage = messages.First();

                sb.AppendLine("using var scope = _scopeFactory.CreateScope();");
                sb.AppendLine($"var mediator = scope.ServiceProvider.GetRequiredService<{AnnotateTypeWithGlobal(_mediatRInterfaceSymbol)}>();");
                sb.AppendLine($"var any = {AnnotateTypeWithGlobal(_protobufAnySymbol)}.Parser.ParseFrom(message);\n");
                sb.AppendLine($"if (any.Is({firstMessage}.Descriptor))");
                sb.AppendLine("{");
                sb.AppendLine(Indent($"var msg = any.Unpack<{firstMessage}>();", 1));
                sb.AppendLine(Indent(
                    $"var req = new global::{GeneratedNamespace}.TcpRequest<{firstMessage}>(connectedClient, msg);", 1));
                sb.AppendLine(Indent("await mediator.Send(req);", 1));
                sb.AppendLine("}");

                for (var i = 1; i < messages.Count; i++)
                {
                    sb.AppendLine($"else if (any.Is({messages[i]}.Descriptor))");
                    sb.AppendLine("{");
                    sb.AppendLine(Indent($"var msg = any.Unpack<{messages[i]}>();", 1));
                    sb.AppendLine(Indent(
                        $"var req = new global::{GeneratedNamespace}.TcpRequest<{messages[i]}>(connectedClient, msg);", 1));
                    sb.AppendLine(Indent("await mediator.Send(req);", 1));
                    sb.AppendLine("}");
                }

                sb.AppendLine("else");
                sb.AppendLine("{");
                sb.AppendLine(Indent($"_logger.LogWarning(\"Received unknown message.\");", 1));
                sb.Append("}");

                return sb.ToString();
            }
        }
    }
}