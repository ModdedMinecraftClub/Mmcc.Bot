using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.Diagnostics
{
    /// <summary>
    /// Pings a network resource.
    /// </summary>
    public class PingNetworkResource
    {
        /// <summary>
        /// Query to ping a network resource.
        /// </summary>
        public class Query : IRequest<Result<QueryResult>>
        {
            /// <summary>
            /// Address of the network resource to ping.
            /// </summary>
            public string Address { get; set; } = null!;
        }
        
        public class QueryResult
        {
            /// <summary>
            /// Address of the pinged network resource.
            /// </summary>
            public string Address { get; set; } = null!;
            
            /// <summary>
            /// Status.
            /// </summary>
            public IPStatus Status { get; set; }
            
            /// <summary>
            /// Roundtrip time in milliseconds.
            /// </summary>
            public long? RoundtripTime { get; set; }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<QueryResult>>
        {
            private const int Timeout = 120;
            
            /// <inheritdoc />
            public async Task<Result<QueryResult>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var ping = new Ping();
                    var options = new PingOptions
                    {
                        DontFragment = true
                    };
                    var buffer = new byte[32];
                    var reply = await ping.SendPingAsync(request.Address, Timeout, buffer, options);

                    return Result<QueryResult>.FromSuccess(new()
                    {
                        Address = reply.Address.ToString(),
                        Status = reply.Status,
                        RoundtripTime = reply.RoundtripTime,
                    });
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}