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

            // beginning of the file up to the method that needs to be generated;
            var sb = new StringBuilder(@"
// auto-generated
namespace Mmcc.Bot.Infrastructure
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

            // loop to create the if chain;
            for (var i = 0; i < messages.Count; i++)
            {
                string innerSb;

                if (i == 0)
                {
                    innerSb = "             " + GenerateIfString(messages[i].Name);
                }
                else
                {
                    innerSb = "             else " + GenerateIfString(messages[i].Name);
                }

                sb.Append(innerSb.ToString());
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

            var str = sb.ToString();
            // inject the created source into the users compilation
            context.AddSource("TcpMessageProcessingService.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
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
