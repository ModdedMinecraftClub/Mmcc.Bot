using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mmcc.Bot.Generators
{
    [Generator]
    public class TcpServiceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!context.Compilation.AssemblyName.Equals("Mmcc.Bot.Infrastructure")) return; 
            
            var messageType = context.Compilation.GetTypeByMetadataName("Google.Protobuf.IMessage");
            var messages = context.Compilation.GlobalNamespace
                .GetNamespaceMembers()
                .FirstOrDefault(n => n.Name.Equals("Mmcc"))
                .GetNamespaceMembers()
                .FirstOrDefault(n => n.Name.Equals("Bot"))
                .GetNamespaceMembers()
                .FirstOrDefault(n => n.Name.Equals("Protos"))
                .GetTypeMembers()
                .Where(t => t.Interfaces.Contains(messageType))
                .ToList();
            
            context.AddSource("TcpMessageProcessingService.cs", SourceText.From(GenerateService(messages), Encoding.UTF8));
        }

        private static string GenerateService(List<INamedTypeSymbol> messages)
        {
            // beginning of the file up to the method that needs to be generated;
            var sb = new StringBuilder(@"
// auto-generated
namespace Mmcc.Bot.Infrastructure.Services
{
    public class TcpMessageProcessingService
    {
        private readonly global::MediatR.IMediator _mediator;
        
        public TcpMessageProcessingService(global::MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public async global::System.Threading.Tasks.Task Handle(global::Ssmp.ConnectedClient connectedClient, byte[] message)
        {");

            sb.Append(@"
             var any = global::Google.Protobuf.WellKnownTypes.Any.Parser.ParseFrom(message);
");
            
            sb.Append("             " + GenerateIfString(messages.First().Name));

            // loop to create the if chain;
            for (var i = 1; i < messages.Count; i++)
            {
                sb.Append("             else " + GenerateIfString(messages[i].Name));
            }

            // else
            sb.Append(@"             else
             {
                   throw new global::System.Exception();
             }
");
            // closing brackets;
            sb.AppendLine(@"        }
    }
}");
            return sb.ToString();
        }

        private static string GenerateIfString(string messageName)
        {
            var innerSb = new StringBuilder();
            innerSb.Append($@"if(any.Is(global::Mmcc.Bot.Protos.{messageName}.Descriptor))
    ");
            innerSb.AppendLine("         {");

            var mediatorSb = new StringBuilder(@"                   await _mediator.Send(any.Unpack");
            mediatorSb.Append($@"<global::Mmcc.Bot.Protos.{messageName}>());");
            innerSb.AppendLine(mediatorSb.ToString());
            innerSb.AppendLine("             }");

            return innerSb.ToString();
        }
    }
}
