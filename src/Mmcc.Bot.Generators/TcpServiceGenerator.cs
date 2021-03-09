using System.Collections.Generic;
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
            if (!context.Compilation.AssemblyName!.Equals("Mmcc.Bot.Infrastructure")) return; 
            
            var messageType = context.Compilation.GetTypeByMetadataName("Google.Protobuf.IMessage");
            var messages = context.Compilation.GlobalNamespace
                .GetNamespaceMembers()
                .FirstOrDefault(n => n.Name.Equals("Mmcc"))?
                .GetNamespaceMembers()
                .FirstOrDefault(n => n.Name.Equals("Bot"))?
                .GetNamespaceMembers()
                .FirstOrDefault(n => n.Name.Equals("Protos"))?
                .GetTypeMembers()
                .Where(t => t.Interfaces.Contains(messageType))
                .ToList();
            
            context.AddSource("TcpMessageProcessingService.cs", SourceText.From(GenerateService(messages), Encoding.UTF8));
        }

        private static string GenerateService(List<INamedTypeSymbol> messages)
        {
            const string interfaceToInherit = "global::Mmcc.Bot.Infrastructure.Services.ITcpMessageProcessingService";
            
            if (messages is null || !messages.Any())
            {
                return $@"
// auto-generated
namespace Mmcc.Bot.Infrastructure.Services
{{
    public class TcpMessageProcessingService : {interfaceToInherit}
    {{
        private readonly global::MediatR.IMediator _mediator;
        
        public TcpMessageProcessingService(global::MediatR.IMediator mediator)
        {{
            _mediator = mediator;
        }}

        public global::System.Threading.Tasks.Task Handle(global::Ssmp.ConnectedClient connectedClient, byte[] message)
        {{
            // generated code will go here;
            return global::System.Threading.Tasks.Task.CompletedTask;
        }}
    }}
}}
";
            }

            // beginning of the file up to the method that needs to be generated;
            var sb = new StringBuilder($@"
// auto-generated
namespace Mmcc.Bot.Infrastructure.Services
{{
    public class TcpMessageProcessingService : {interfaceToInherit}
    {{
        private readonly global::MediatR.IMediator _mediator;
        
        public TcpMessageProcessingService(global::MediatR.IMediator mediator)
        {{
            _mediator = mediator;
        }}
        
        public async global::System.Threading.Tasks.Task Handle(global::Ssmp.ConnectedClient connectedClient, byte[] message)
        {{");

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
                   throw new global::System.Exception(""Unknown message type."");
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
            var messageTypeWithNamespace = $"global::Mmcc.Bot.Protos.{messageName}";
            var innerSb = new StringBuilder();
    
            innerSb.Append($@"if(any.Is({messageTypeWithNamespace}.Descriptor))
    ");
            innerSb.AppendLine("         {");    
    
            var mediatorSb = new StringBuilder(@$"                   var unpackedMsg = any.Unpack<{messageTypeWithNamespace}>();");
    
            mediatorSb.AppendLine();
            mediatorSb.AppendLine(@$"                   var request = new global::Mmcc.Bot.Infrastructure.Requests.Generic.TcpRequest<{messageTypeWithNamespace}>(connectedClient, unpackedMsg);");
            mediatorSb.AppendLine(@"                   await _mediator.Send(request);");
            innerSb.Append(mediatorSb);
            innerSb.AppendLine("             }");
    
            return innerSb.ToString();
        }
    }
}
