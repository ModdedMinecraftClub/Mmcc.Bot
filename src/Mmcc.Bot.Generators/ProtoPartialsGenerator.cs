using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mmcc.Bot.Generators
{
    [Generator]
    public class ProtoPartialsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (SyntaxReceiver) context.SyntaxContextReceiver;
            var classes = receiver?.Classes;
            var assemblyName = context.Compilation.AssemblyName;

            if (classes is null || !classes.Any()) return;

            context.AddSource("RequestPartials.cs", SourceText.From(GeneratePartialsFileString(classes, assemblyName), Encoding.UTF8));
        }
        
        private static string GeneratePartialsFileString(IEnumerable<ClassDeclarationSyntax> classes, string assemblyName)
        {
            var sb = new StringBuilder($@"
namespace {assemblyName}
{{
");
            foreach (var c in classes)
            {
                sb.AppendLine($"    public sealed partial class {c.Identifier} : global::MediatR.IRequest {{}}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            public SyntaxReceiver()
            {
                Classes = new();
            }

            public List<ClassDeclarationSyntax> Classes { get; }

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                var messageType = context.SemanticModel.Compilation.GetTypeByMetadataName("Google.Protobuf.IBufferMessage");

                if (
                    context.Node is ClassDeclarationSyntax classDeclaration
                    && classDeclaration.BaseList is BaseListSyntax baseList
                    && baseList.Types.Any(t => IsModuleClass(t, context.SemanticModel, messageType))
                )
                {
                    Classes.Add(classDeclaration);
                }
            }
            
            private static bool IsModuleClass(BaseTypeSyntax baseType,
                SemanticModel model, INamedTypeSymbol moduleType)
            {
                var typeInfo = model.GetTypeInfo(baseType.Type);

                return SymbolEqualityComparer.Default
                    .Equals(typeInfo.Type, moduleType);
            }
        }
    }
}