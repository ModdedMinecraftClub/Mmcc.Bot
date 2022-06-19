using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mmcc.Bot.Generators
{
    [Generator]
    public class RequestResolverGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) =>
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (SyntaxReceiver)context.SyntaxContextReceiver!;
            var templateGenerator =
                new TemplateGenerator(receiver.PolychatRequestInterfaceSymbol!, receiver.PolychatMessageClasses);

            var generatedCode = templateGenerator.Generate();
            
            context.AddSource("RequestResolverImpl.cs", generatedCode);
        }

        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            private INamedTypeSymbol? _protoMessageInterfaceSymbol;
            
            public SyntaxReceiver() =>
                PolychatMessageClasses = new();
            
            public INamedTypeSymbol? PolychatRequestInterfaceSymbol { get; private set; }

            public List<ClassDeclarationSyntax> PolychatMessageClasses { get; }

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                _protoMessageInterfaceSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("Google.Protobuf.IBufferMessage");
                
                PolychatRequestInterfaceSymbol ??= context.SemanticModel.Compilation.GetTypeByMetadataName("Mmcc.Bot.Polychat.Networking.IPolychatRequest");

                if (context.Node is ClassDeclarationSyntax { BaseList: { } baseList } cds
                    && baseList
                        .Types
                        .Any(t => IsModuleClass(t, context.SemanticModel, _protoMessageInterfaceSymbol!)))
                {
                    PolychatMessageClasses.Add(cds);
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
            private readonly string GENERATED_NAMESPACE = "Mmcc.Bot.Polychat.Networking";
            private readonly string MSG_NAMESPACE = "Mmcc.Bot.Polychat";
            private readonly string GENERATED_CLASS = "RequestResolver";
            
            private readonly INamedTypeSymbol _polychatRequestInterfaceSymbol;

            private readonly List<ClassDeclarationSyntax> _polychatMessageClasses;

            public TemplateGenerator(
                INamedTypeSymbol polychatRequestInterfaceSymbol,
                List<ClassDeclarationSyntax> polychatMessageClasses
            )
            {
                _polychatRequestInterfaceSymbol = polychatRequestInterfaceSymbol;
                _polychatMessageClasses = polychatMessageClasses;
            }
            
            private string AnnotateMsgType(string type) => $"global::{MSG_NAMESPACE}.{type}";

            protected override string FillInStub(string generatedFiller)
                => $@"// auto-generated
namespace {GENERATED_NAMESPACE};

#pragma warning disable CS0612 // type is obsolete
public partial class {GENERATED_CLASS}
{{
    public {AnnotateTypeWithGlobal(_polychatRequestInterfaceSymbol)}? Resolve()
    {{
{string.Join("\n", generatedFiller.Split('\n').Select(s => Indent(s, 2)))}
    }}
}}
#pragma warning restore CS0612
";

            protected override string GenerateFiller()
            {
                if (!_polychatMessageClasses.Any())
                {
                    return GenerateNoClassesStub();
                }
                
                var ifStatements = _polychatMessageClasses
                    .Select(c => AnnotateMsgType(c.Identifier.ValueText))
                    .Select(GenerateIfForMsgClass);

                var ifStatementsStr = string.Join("\n", ifStatements);

                var sb = new StringBuilder();

                sb.AppendLine(ifStatementsStr);
                sb.AppendLine("return null;");

                return sb.ToString();
            }

            private static string GenerateNoClassesStub()
                => new StringBuilder()
                    .AppendLine("// no Polychat classes found;")
                    .AppendLine("// once classes are added to the Polychat project generated code will go here;")
                    .AppendLine()
                    .AppendLine("return null;")
                    .ToString();

            private string GenerateIfForMsgClass(string messageType)
                => new StringBuilder()
                    .AppendLine($"if (_msgContext.MessageContent!.Is({messageType}.Descriptor))")
                    .AppendLine("{")
                    .AppendLine(Indent($"var msg = _msgContext.MessageContent!.Unpack<{messageType}>();", 1))
                    .AppendLine(Indent($"return new global::Mmcc.Bot.Polychat.Networking.PolychatRequest<{messageType}>(_msgContext.Author!, msg);", 1))
                    .AppendLine("}")
                    .ToString();
        }
    }
}